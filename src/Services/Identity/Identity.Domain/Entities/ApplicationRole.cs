using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
