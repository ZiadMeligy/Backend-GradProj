
using GP_Server.Application.Services;
using GP_Server.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GP_Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ProfileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(IFileService fileService, UserManager<ApplicationUser> userManager)
        {
            _fileService = fileService;
            _userManager = userManager;
        }

        [HttpPost("upload-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Invalid file.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound("User not found");

                var profilePicturePath = await _fileService.SaveProfilePictureAsync(file, userId);

                // Update user profile with new picture path
                user.ProfilePicturePath = profilePicturePath;
                await _userManager.UpdateAsync(user);

                return Ok(new { profilePicturePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
