using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GP_Server.Application.Interfaces;
using GP_Server.Application.ApiResponses;
using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Application.Helpers;
using GP_Server.Application.DTOs.Reports;

namespace GP_Server.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class DoctorController : ControllerBase
    {
        private readonly ILogger<DoctorController> _logger;
        private readonly IDoctorService _doctorService;

        public DoctorController(ILogger<DoctorController> logger, IDoctorService doctorService)
        {
            _logger = logger;
            _doctorService = doctorService;
        }

        /// <summary>
        /// Get all studies assigned to the logged-in doctor
        /// </summary>
        /// <returns>List of studies assigned to the current doctor</returns>
        [HttpGet("my-studies")]
        public async Task<IActionResult> GetMyAssignedStudiesAsync()
        {
            try
            {
                // Get the current doctor's ID from the JWT token
                var doctorId = UserHelper.GetUserId(User);

                _logger.LogInformation("Fetching studies for doctor: {DoctorId}", doctorId);

                var studies = await _doctorService.GetStudiesAssignedToDoctorAsync(doctorId);

                return new ApiResponse<List<StudyStatusDTO>>(
                    data: studies,
                    message: "Studies retrieved successfully",
                    statusCode: StatusCodes.Status200OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching studies for logged-in doctor");
                throw;
            }
        }

        /// <summary>
        /// Get all studies assigned to the logged-in doctor with full Orthanc data (patient, study, status)
        /// </summary>
        /// <param name="pagination">Pagination parameters</param>
        /// <returns>List of studies with full patient information and status assigned to the current doctor</returns>
        [HttpGet("my-studies-full")]
        public async Task<IActionResult> GetMyAssignedStudiesWithFullDataAsync([FromQuery] PaginationParameters? pagination = null)
        {
            try
            {
                // Get the current doctor's ID from the JWT token
                var doctorId = UserHelper.GetUserId(User);

                _logger.LogInformation("Fetching studies with full data for doctor: {DoctorId}", doctorId);

                var (studies, totalCount) = await _doctorService.GetStudiesAssignedToDoctorWithFullDataAsync(doctorId, pagination);

                if (pagination != null)
                {
                    return new PaginatedResponse<List<object>>(
                        data: studies,
                        totalRecords: totalCount,
                        pageNumber: pagination.PageNumber,
                        pageSize: pagination.PageSize,
                        message: "Studies with full data retrieved successfully",
                        statusCode: StatusCodes.Status200OK
                    );
                }

                return new ApiResponse<List<object>>(
                    data: studies,
                    message: "Studies with full data retrieved successfully",
                    statusCode: StatusCodes.Status200OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching studies with full data for logged-in doctor");
                throw;
            }
        }        [HttpPost("review-study/{studyId}")]
        public async Task<IActionResult> ReviewStudyAsync(string studyId, [FromBody] ReviewReportDTO reviewReportDTO)
        {
            try
            {
                // Get the current doctor's ID from the JWT token
                var doctorId = UserHelper.GetUserId(User);

                _logger.LogInformation("Doctor {DoctorId} is reviewing study {StudyId}", doctorId, studyId);

                var updatedStudy = await _doctorService.ReviewReportAsync(studyId, doctorId, reviewReportDTO);

                return new ApiResponse<StudyStatusDTO>(
                    data: updatedStudy,
                    message: "Review submitted successfully",
                    statusCode: StatusCodes.Status200OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing study {StudyId} by doctor {DoctorId}", studyId, UserHelper.GetUserId(User));
                throw;
            }
        }
    }
}
