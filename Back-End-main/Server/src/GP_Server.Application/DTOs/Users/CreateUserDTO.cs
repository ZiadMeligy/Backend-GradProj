using System;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Application.DTOs.Users;

public class CreateUserDTO : UserDTO
{
    public string Password { get; set; } = string.Empty;
    public List<Guid> Roles { get; set; } = new List<Guid>();
    
}
