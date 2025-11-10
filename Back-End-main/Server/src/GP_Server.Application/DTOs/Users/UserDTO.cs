using System;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Application.DTOs.Users;

public abstract class UserDTO
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [RegularExpression("^(Male|Female)$", ErrorMessage = "There are only two")]
    public string Gender { get; set; } = string.Empty;
    public string SSN { get; set; } = string.Empty;
}
