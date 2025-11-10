using System;

namespace GP_Server.Application.DTOs.Roles;

public class CreateRoleDTO : RoleDTO
{
    public List<Guid> Users { get; set; } = new List<Guid>();
}
