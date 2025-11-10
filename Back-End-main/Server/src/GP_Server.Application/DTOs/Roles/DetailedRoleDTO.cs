using System;
using GP_Server.Application.DTOs.Users;

namespace GP_Server.Application.DTOs.Roles;

public class DetailedRoleDTO : GeneralRoleDTO
{
    public List<GeneralUserDTO> Users { get; set; } = new List<GeneralUserDTO>();
}

