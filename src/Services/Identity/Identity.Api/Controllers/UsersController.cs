using Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var q = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u => u.Email!.ToLower().Contains(s)
                || u.FirstName.ToLower().Contains(s)
                || u.LastName.ToLower().Contains(s));
        }

        var users = await q.OrderBy(u => u.Email).Take(500).ToListAsync(ct);

        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new
            {
                id = u.Id,
                email = u.Email,
                firstName = u.FirstName,
                lastName = u.LastName,
                fullName = $"{u.FirstName} {u.LastName}".Trim(),
                phoneNumber = u.PhoneNumber,
                isActive = u.IsActive,
                isSeller = u.IsSeller,
                createdAt = u.CreatedAt,
                lastLoginAt = (DateTime?)null,
                roles
            });
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var u = await _userManager.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(u);

        return Ok(new
        {
            id = u.Id,
            email = u.Email,
            firstName = u.FirstName,
            lastName = u.LastName,
            fullName = $"{u.FirstName} {u.LastName}".Trim(),
            phoneNumber = u.PhoneNumber,
            isActive = u.IsActive,
            isSeller = u.IsSeller,
            createdAt = u.CreatedAt,
            roles
        });
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var u = await _userManager.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        u.IsActive = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(u);

        return Ok(new { message = "کاربر مسدود شد" });
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var u = await _userManager.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        u.IsActive = true;
        u.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(u);

        return Ok(new { message = "کاربر فعال شد" });
    }
}
