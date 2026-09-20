using Identity.Application.Auth.Dtos;
using Identity.Domain.Entities;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly OtpService _otpService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        TokenService tokenService,
        OtpService otpService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _otpService = otpService;
    }

    // ===== ثبت‌نام (بدون تغییر) =====
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            return Conflict("Email already registered.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (!await _roleManager.RoleExistsAsync("Customer"))
            await _roleManager.CreateAsync(new ApplicationRole("Customer"));

        await _userManager.AddToRoleAsync(user, "Customer");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!,
            $"{user.FirstName} {user.LastName}".Trim(), expiresAt));
    }

    // ===== لاگین با رمز (بدون تغییر) =====
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.IsActive)
            return Unauthorized("Invalid credentials.");

        var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
            return Unauthorized("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!,
            $"{user.FirstName} {user.LastName}".Trim(), expiresAt));
    }

    // ===== OTP =====

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(
        [FromBody] SendOtpRequestDto dto,
        CancellationToken ct)
    {
        var (success, message, expiresAt) = await _otpService.SendOtpAsync(dto.Email, ct);

        if (!success)
            return BadRequest(new { message });

        return Ok(new SendOtpResponseDto(
            message,
            expiresAt ?? DateTime.UtcNow.AddMinutes(5)));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequestDto dto,
        CancellationToken ct)
    {
        var user = await _otpService.VerifyOtpAsync(dto.Email, dto.Code, ct);

        if (user is null)
            return Unauthorized("کد نامعتبر یا منقضی شده است.");

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        return Ok(new OtpLoginResponseDto(
            token,
            user.Email!,
            $"{user.FirstName} {user.LastName}".Trim(),
            expiresAt));
    }
}
