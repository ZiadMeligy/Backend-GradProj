using System;
using GP_Server.Application.DTOs.Roles;

namespace GP_Server.Application.DTOs.Users;

public class DetailedUserDTO : GeneralUserDTO
{
    public List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
    public string ProfilePicturePath { get; set; } = string.Empty;
}
