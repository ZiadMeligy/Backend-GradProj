using System;

namespace GP_Server.Application.DTOs.Users;

public class GeneralUserDTO : UserDTO
{
    public Guid Id { get; set; }
    public string ProfilePicturePath { get; set; } = string.Empty; 
}
