using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Domain.Entities;

public class ApplicationUser : IdentityUser
{
        public DateTime? CreatedAt { get; set; }

        public ApplicationUser() : base()
        {
            CreatedAt = DateTime.UtcNow;
            ProfilePicturePath = "images/profile/default-profile.jpg"; // Default profile picture
        }
        public string? SSN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [RegularExpression("^(Male|Female)$", ErrorMessage = "There are only two Genders")]
        public string Gender { get; set; } = string.Empty;
        public string ProfilePicturePath { get; set; } = string.Empty;
}
