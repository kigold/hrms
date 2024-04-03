﻿using Microsoft.AspNetCore.Identity;
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
    }

    public class Role : IdentityRole<long>
    {
    }

    public class RoleClaim : IdentityRoleClaim<long> { }
    public class UserClaim : IdentityUserClaim<long> { }
    public class UserRole : IdentityUserRole<long> { }
}
