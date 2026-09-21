using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Dtos;
using Storage.Domain.Entities;
using Storage.Infrastructure.Data;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage/folders")]
[Authorize]
public sealed class FoldersController : ControllerBase
{
    private readonly StorageDbContext _db;
    public FoldersController(StorageDbContext db) => _db = db;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? parentId, CancellationToken ct)
    {
        var q = _db.Folders.AsNoTracking()
            .Where(f => f.OwnerId == UserId && !f.IsDeleted);

        q = parentId.HasValue
            ? q.Where(f => f.ParentFolderId == parentId)
            : q.Where(f => f.ParentFolderId == null);

        var folders = await q.OrderBy(f => f.Name)
            .Select(f => new FolderDto(f.Id, f.OwnerId, f.ParentFolderId, f.Name,
                f.Files.Count(x => !x.IsDeleted),
                f.Children.Count(c => !c.IsDeleted),
                f.CreatedAt))
            .ToListAsync(ct);

        return Ok(folders);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFolderDto dto, CancellationToken ct)
    {
        if (dto.ParentFolderId.HasValue)
        {
            var parentExists = await _db.Folders
                .AnyAsync(f => f.Id == dto.ParentFolderId && f.OwnerId == UserId && !f.IsDeleted, ct);
            if (!parentExists) return BadRequest("پوشه والد یافت نشد");
        }

        var folder = new Folder(UserId, dto.Name, dto.ParentFolderId);
        _db.Folders.Add(folder);
        await _db.SaveChangesAsync(ct);

        return Ok(new FolderDto(folder.Id, folder.OwnerId, folder.ParentFolderId,
            folder.Name, 0, 0, folder.CreatedAt));
    }

    [HttpPut("{id:guid}/rename")]
    public async Task<IActionResult> Rename(Guid id, RenameFolderDto dto, CancellationToken ct)
    {
        var folder = await _db.Folders.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);
        if (folder is null) return NotFound();
        if (folder.OwnerId != UserId) return Forbid();

        folder.Rename(dto.Name);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var folder = await _db.Folders.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);
        if (folder is null) return NotFound();
        if (folder.OwnerId != UserId) return Forbid();

        folder.SoftDelete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
