namespace Storage.Domain.Entities;

public sealed class Group
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsSystemGroup { get; private set; }

    // ===== Password Policy =====
    public int MinPasswordLength { get; private set; } = 8;
    public bool RequireUppercase { get; private set; } = true;
    public bool RequireDigit { get; private set; } = true;
    public bool RequireLowercase { get; private set; } = true;
    public bool RequireSpecialChar { get; private set; } = false;
    public int? PasswordExpiryDays { get; private set; } = 90;
    public int? MaxLoginAttempts { get; private set; } = 5;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();
    public ICollection<GroupPermission> Permissions { get; private set; } = new List<GroupPermission>();

    private Group() { }

    public Group(string name, Guid createdBy, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.");
        Name = name.Trim();
        CreatedByUserId = createdBy;
        Description = description;
        IsSystemGroup = isSystem;
    }

    public void Rename(string newName) { Name = newName.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void UpdateDescription(string? desc) { Description = desc; UpdatedAt = DateTime.UtcNow; }

    public void SetPasswordPolicy(int minLength, bool requireUpper, bool requireDigit,
        bool requireLower, bool requireSpecial, int? expiryDays, int? maxAttempts)
    {
        MinPasswordLength = Math.Clamp(minLength, 4, 64);
        RequireUppercase = requireUpper;
        RequireDigit = requireDigit;
        RequireLowercase = requireLower;
        RequireSpecialChar = requireSpecial;
        PasswordExpiryDays = expiryDays;
        MaxLoginAttempts = maxAttempts;
        UpdatedAt = DateTime.UtcNow;
    }
}
