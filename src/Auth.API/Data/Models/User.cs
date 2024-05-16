using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.API.Data.Models
{
    public class User : IdentityUser<long>
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string? Avatar { get; set; }
        [NotMapped]
        public string FullName => $"{Firstname} {Lastname}";
        [NotMapped]
        public bool IsActive => LockoutEnd == null;
    }

    public class Role : IdentityRole<long>
    {
    }

    public class RoleClaim : IdentityRoleClaim<long> { }
    public class UserClaim : IdentityUserClaim<long> { }
    public class UserRole : IdentityUserRole<long> { }

    public class ClaimDTO
    {
        public string ClaimValue { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
    }
}
