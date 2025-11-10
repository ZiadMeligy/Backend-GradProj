using GP_Server.Application.Interfaces;
using GP_Server.Application.DTOs.Roles;
using GP_Server.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using GP_Server.Domain.Interfaces;
using GP_Server.Application.Exceptions;
using AutoMapper;
using GP_Server.Application.DTOs.Users;

namespace GP_Server.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IRepository<Role> roleRepository, RoleManager<Role> roleManager, IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task CreateRoleAsync(CreateRoleDTO roleDTO)
    {
        var role = new Role { Name = roleDTO.Name };
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var users = await _validateUsers(roleDTO.Users);
        foreach (var user in users)
            await _userManager.AddToRoleAsync(user, role.Name);
    }

    public async Task DeleteRoleAsync(string id)
    {
        var role = await _getRoleByIdAsync(id);
        await _roleManager.DeleteAsync(role);
    }

    public async Task<IEnumerable<DetailedRoleDTO>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var rolesDTO = _mapper.Map<IEnumerable<DetailedRoleDTO>>(roles);
        foreach (var role in rolesDTO)
            role.Users = _mapper.Map<List<GeneralUserDTO>>(await _userManager.GetUsersInRoleAsync(role.Name));

        return rolesDTO;
        
    }

    public async Task<DetailedRoleDTO> GetRoleByIdAsync(string id)
    {
        var role = await _getRoleByIdAsync(id);
        var roleDTO = _mapper.Map<DetailedRoleDTO>(role);
        roleDTO.Users = _mapper.Map<List<GeneralUserDTO>>(await _userManager.GetUsersInRoleAsync(role.Name));

        return roleDTO;
    }

    public async Task UpdateRoleAsync(string id, CreateRoleDTO roleDTO)
    {
        var role = await _getRoleByIdAsync(id);
        role.Name = roleDTO.Name;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var users = await _validateUsers(roleDTO.Users);
        var currentUsers = await _userManager.GetUsersInRoleAsync(role.Name);

        var usersToAdd = users.Except(currentUsers).ToList();
        var usersToRemove = currentUsers.Except(users).ToList();

        foreach (var user in usersToAdd)
            await _userManager.AddToRoleAsync(user, role.Name);

        foreach (var user in usersToRemove)
            await _userManager.RemoveFromRoleAsync(user, role.Name);
    }

    // 🔹 Helper Methods to Remove Duplication 🔹

    private async Task<Role> _getRoleByIdAsync(string id)
    {
        return await _roleManager.FindByIdAsync(id) ?? throw new NotFoundException("Role not found");
    }

    private async Task<List<ApplicationUser>> _validateUsers(IEnumerable<Guid> userIds)
    {
        var users = await Task.WhenAll(userIds.Select(async id => await _userManager.FindByIdAsync(id.ToString())));
        if (users.Any(user => user == null))
            throw new NotFoundException("One or more users not found");

        return users.ToList()!;
    }
}
