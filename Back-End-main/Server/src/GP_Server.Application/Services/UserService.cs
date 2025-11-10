using System.Linq.Expressions;
using AutoMapper;
using GP_Server.Application.DTOs.Roles;
using GP_Server.Application.DTOs.Users;
using GP_Server.Application.Exceptions;
using GP_Server.Application.Interfaces;
using GP_Server.Domain.Entities;
using GP_Server.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GP_Server.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager, IRepository<ApplicationUser> userRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateUserAsync(CreateUserDTO user)
    {
        if (await _userManager.FindByEmailAsync(user.Email) != null)
            throw new BadRequestException("User with this email already exists");

        var newUser = _mapper.Map<ApplicationUser>(user);
        newUser.UserName = user.Email;
        var result = await _userManager.CreateAsync(newUser, user.Password);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var userRoles = _validateRoles(user.Roles);
        if (userRoles.Any())
            await _userManager.AddToRolesAsync(newUser, userRoles.Select(r => r.Name!));
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _getUserByIdAsync(id);
        await _userManager.DeleteAsync(user);
    }

    public async Task<DetailedUserDTO> GetUserByIdAsync(string id)
    {
        var user = await _getUserByIdAsync(id);
        var userDto = _mapper.Map<DetailedUserDTO>(user);
        userDto.Roles = _mapper.Map<List<RoleDTO>>(await _userManager.GetRolesAsync(user));
        return userDto;
    }

    public async Task<IEnumerable<DetailedUserDTO>> GetUsersAsync(FilteredDTO filter)
    {
        Expression<Func<ApplicationUser, bool>> predicate = u =>
        (string.IsNullOrEmpty(filter.FirstName) || u.FirstName.Contains(filter.FirstName)) &&
        (string.IsNullOrEmpty(filter.LastName) || u.LastName.Contains(filter.LastName));

        var users = await _userRepository.FindAsync(predicate);

        // If no filters are applied, return all users
        if (string.IsNullOrEmpty(filter.FirstName) && string.IsNullOrEmpty(filter.LastName))
        {
            users = await _userRepository.GetAllAsync();
        }

        var userDtos = _mapper.Map<IEnumerable<DetailedUserDTO>>(users);

        // Map roles
        foreach (var userDto in userDtos)
        {
            var user = users.First(u => u.Id == userDto.Id.ToString());
            userDto.Roles = _mapper.Map<List<RoleDTO>>(await _userManager.GetRolesAsync(user));
        }

        return userDtos;
    
        
    }

    public async Task<IEnumerable<DetailedUserDTO>> GetUsersWhereAsync(Expression<Func<ApplicationUser, bool>> predicate)
    {
        var users = await _userRepository.FindAsync(predicate);
        var userDtos = _mapper.Map<IEnumerable<DetailedUserDTO>>(users);
        // map roles
        foreach (var userDto in userDtos)
        {
            var user = users.First(u => u.Id == userDto.Id.ToString());
            userDto.Roles = _mapper.Map<List<RoleDTO>>(await _userManager.GetRolesAsync(user));
        }
        return userDtos;
    }

    public async Task UpdateUserAsync(CreateUserDTO userDto, Guid id)
    {
        var user = await _getUserByIdAsync(id.ToString());

        _mapper.Map(userDto, user); // Automatically maps properties

        if (userDto.Roles.Any())
        {
            var userRoles =  _validateRoles(userDto.Roles);
            await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
            await _userManager.AddToRolesAsync(user, userRoles.Select(r => r.Name!));
        }
    }

    // 🔹 Helper Methods to Remove Duplication 🔹

    private async Task<ApplicationUser> _getUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id) ?? throw new NotFoundException("User not found");
    }
    private List<Role> _validateRoles(IEnumerable<Guid> roleIds)
    {
        if (!roleIds.Any())
            return new List<Role>();
        var roles = _roleManager.Roles.ToList();
        roles = roles.Where(r => roleIds.Contains(Guid.Parse(r.Id))).ToList();
        

        if (roleIds.Count() != roles.Count)
            throw new BadRequestException("One or more roles not found");

        return roles;
    }
}
