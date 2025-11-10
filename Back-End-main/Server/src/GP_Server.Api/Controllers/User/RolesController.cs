using GP_Server.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GP_Server.Application.ApiResponses;
using GP_Server.Application.DTOs.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GP_Server.Api.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleDTO role)
        {
            await _roleService.CreateRoleAsync(role);
            return new ApiResponse<object?>(data: null, message: "Role created successfully", statusCode: StatusCodes.Status201Created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleAsync(string id)
        {
            await _roleService.DeleteRoleAsync(id);
            return new ApiResponse<object?>(data: null, message: "Role deleted successfully", statusCode: StatusCodes.Status200OK);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleAsync(string id, [FromBody] CreateRoleDTO role)
        {
            await _roleService.UpdateRoleAsync(id, role);
            return new ApiResponse<object?>(data: null, message: "Role updated successfully", statusCode: StatusCodes.Status200OK);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return new ApiResponse<IEnumerable<DetailedRoleDTO>>(data: roles, message: "Roles retrieved successfully", statusCode: StatusCodes.Status200OK);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleByIdAsync(string id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return new ApiResponse<DetailedRoleDTO>(data: role, message: "Role retrieved successfully", statusCode: StatusCodes.Status200OK);
        }

    }
}
