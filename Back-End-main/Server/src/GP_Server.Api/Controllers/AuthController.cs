using GP_Server.Application.ApiResponses;
using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Auth;
using GP_Server.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GP_Server.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDTO login)
        {
            var token = await _authService.LoginAsync(login);
            return new ApiResponse<TokenDTO>(data: token, message: "User logged in successfully", statusCode: StatusCodes.Status200OK);
        }
    }
}
