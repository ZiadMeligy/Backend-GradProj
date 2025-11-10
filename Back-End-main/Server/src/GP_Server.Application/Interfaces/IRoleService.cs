using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GP_Server.Application.DTOs.Roles;
using GP_Server.Domain.Entities;

namespace GP_Server.Application.Interfaces;

    public interface IRoleService
    {
        Task<IEnumerable<DetailedRoleDTO>> GetAllRolesAsync();
        Task<DetailedRoleDTO> GetRoleByIdAsync(string id);
        Task CreateRoleAsync(CreateRoleDTO roleDTO);
        Task UpdateRoleAsync(string id, CreateRoleDTO roleDTO);
        Task DeleteRoleAsync(string id);
    }   

