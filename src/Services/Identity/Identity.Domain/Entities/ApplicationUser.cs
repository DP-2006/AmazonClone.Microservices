using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ===== آدرس =====
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    // ===== فروشنده =====
    public bool IsSeller { get; set; } = false;
    public string? BusinessName { get; set; }
    public string? BusinessType { get; set; }
    public string? BusinessDescription { get; set; }
    public string? BusinessWebsite { get; set; }
}
