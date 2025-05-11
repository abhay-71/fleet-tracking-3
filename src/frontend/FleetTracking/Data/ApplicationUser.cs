using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace FleetTracking.Data;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
    public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }
    public virtual ICollection<IdentityUserToken<string>> Tokens { get; set; }
    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }

    public ApplicationUser()
    {
        Claims = new HashSet<IdentityUserClaim<string>>();
        Logins = new HashSet<IdentityUserLogin<string>>();
        Tokens = new HashSet<IdentityUserToken<string>>();
        UserRoles = new HashSet<IdentityUserRole<string>>();
    }
    
    // Helper properties
    public string FullName => $"{FirstName} {LastName}";
} 