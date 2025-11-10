using System;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Application.DTOs;

public class LoginDTO
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

}
