using System.Security.Claims;
using Identity.Application.Profile.Dtos;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return NotFound();

        return Ok(new UserProfileDto(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.PhoneNumber, user.PostalCode, user.Address,
            user.City, user.State, user.Country,
            user.IsSeller, user.CreatedAt));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return NotFound();

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.PostalCode = dto.PostalCode?.Trim();
        user.Address = dto.Address?.Trim();
        user.City = dto.City?.Trim();
        user.State = dto.State?.Trim();
        user.Country = dto.Country?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return NotFound();

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid) return BadRequest("رمز عبور اشتباه است.");

        var existing = await _userManager.FindByEmailAsync(dto.NewEmail);
        if (existing is not null) return Conflict("این ایمیل قبلاً استفاده شده است.");

        user.Email = dto.NewEmail.Trim().ToLowerInvariant();
        user.UserName = user.Email;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("become-seller")]
    public async Task<IActionResult> BecomeSeller(BecomeSellerDto dto)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return NotFound();

        if (user.IsSeller) return BadRequest("شما قبلاً فروشنده هستید.");

        user.IsSeller = true;
        user.BusinessName = dto.BusinessName.Trim();
        user.BusinessType = dto.BusinessType;
        user.BusinessDescription = dto.Description.Trim();
        user.BusinessWebsite = dto.Website?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        if (!await _roleManager.RoleExistsAsync("Seller"))
            await _roleManager.CreateAsync(new ApplicationRole("Seller"));

        await _userManager.AddToRoleAsync(user, "Seller");

        return Ok(new { message = "شما با موفقیت فروشنده شدید." });
    }
}
