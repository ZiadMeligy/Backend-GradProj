using GP_Server.Application.ApiResponses;
using GP_Server.Application.DTOs.Users;
using GP_Server.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP_Server.Api.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDTO user)
        {
            await _userService.CreateUserAsync(user);
            return new ApiResponse<object?>(data: null, message: "User created successfully",statusCode: StatusCodes.Status201Created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            await _userService.DeleteUserAsync(id.ToString());
            return new ApiResponse<object?>(data: null, message: "User deleted successfully",statusCode: StatusCodes.Status200OK);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync([FromQuery] FilteredDTO filter)
        {
            var users = await _userService.GetUsersAsync(filter);
            return new ApiResponse<IEnumerable<DetailedUserDTO>>(data: users, message: "Users retrieved successfully",statusCode: StatusCodes.Status200OK);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id.ToString());
            return new ApiResponse<DetailedUserDTO>(data: user, message: "User retrieved successfully",statusCode: StatusCodes.Status200OK);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] CreateUserDTO user)
        {
            await _userService.UpdateUserAsync(user,id);
            return new ApiResponse<object?>(data: null, message: "User updated successfully",statusCode: StatusCodes.Status200OK);
        }

    }
}
