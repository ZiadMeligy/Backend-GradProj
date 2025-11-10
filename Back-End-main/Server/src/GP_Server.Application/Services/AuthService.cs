using System;
using System.Security.Claims;
using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Auth;
using GP_Server.Application.Exceptions;
using GP_Server.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GP_Server.Application.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly TokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager, TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    public async Task<TokenDTO> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _userManager.FindByEmailAsync(loginDTO.Email);
        if (user == null)
            throw new BadRequestException("Invalid email or password");

        var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
        if (!result)
            throw new BadRequestException("Invalid email or password");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.UserName!)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = _tokenService.GenerateToken(claims);
        return new TokenDTO { Token = token };

    }

}
