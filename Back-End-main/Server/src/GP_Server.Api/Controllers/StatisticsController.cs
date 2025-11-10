using GP_Server.Application.ApiResponses;
using GP_Server.Application.DTOs.Statistics;
using GP_Server.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP_Server.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    /// <summary>
    /// Parse time period query parameter
    /// </summary>
    private TimePeriod? ParseTimePeriod(string? timePeriod)
    {
        if (string.IsNullOrEmpty(timePeriod))
            return null;

        if (Enum.TryParse<TimePeriod>(timePeriod, true, out var period))
            return period;

        return null;
    }

    /// <summary>
    /// Get general statistics including number of doctors, patients, and studies
    /// </summary>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>General statistics</returns>
    [HttpGet("general")]
    public async Task<IActionResult> GetGeneralStatsAsync([FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var stats = await _statisticsService.GetGeneralStatsAsync(period);

            return new ApiResponse<GeneralStatsDTO>(
                data: stats,
                message: "General statistics retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving general statistics");
            return new ApiResponse<GeneralStatsDTO?>(
                data: null,
                message: "An error occurred while retrieving general statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get report statistics showing count for each report status
    /// </summary>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>Report statistics</returns>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReportStatsAsync([FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var stats = await _statisticsService.GetReportStatsAsync(period);

            return new ApiResponse<ReportStatsDTO>(
                data: stats,
                message: "Report statistics retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report statistics");
            return new ApiResponse<ReportStatsDTO?>(
                data: null,
                message: "An error occurred while retrieving report statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get doctor statistics showing assigned, finished, and waiting reports for each doctor
    /// </summary>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>Doctor statistics</returns>
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctorStatsAsync([FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var stats = await _statisticsService.GetDoctorStatsAsync(period);

            return new ApiResponse<List<DoctorStatsDTO>>(
                data: stats,
                message: "Doctor statistics retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor statistics");
            return new ApiResponse<List<DoctorStatsDTO>?>(
                data: null,
                message: "An error occurred while retrieving doctor statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get statistics for a specific doctor by their ID
    /// </summary>
    /// <param name="doctorId">The ID of the doctor</param>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>Doctor statistics</returns>
    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetDoctorStatsByIdAsync(string doctorId, [FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var stats = await _statisticsService.GetDoctorStatsByIdAsync(doctorId, period);

            if (stats == null)
            {
                return new ApiResponse<DoctorStatsDTO?>(
                    data: null,
                    message: "Doctor not found or user is not in Doctor role",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return new ApiResponse<DoctorStatsDTO>(
                data: stats,
                message: "Doctor statistics retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for doctor: {DoctorId}", doctorId);
            return new ApiResponse<DoctorStatsDTO?>(
                data: null,
                message: "An error occurred while retrieving doctor statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all statistics in one call (general, report, and doctor statistics)
    /// </summary>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>Complete statistics dashboard data</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllStatisticsAsync([FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var stats = await _statisticsService.GetAllStatisticsAsync(period);

            return new ApiResponse<StatisticsDTO>(
                data: stats,
                message: "All statistics retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all statistics");
            return new ApiResponse<StatisticsDTO?>(
                data: null,
                message: "An error occurred while retrieving all statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get dashboard summary with key metrics
    /// </summary>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>Dashboard summary statistics</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardSummaryAsync([FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var generalStats = await _statisticsService.GetGeneralStatsAsync(period);
            var reportStats = await _statisticsService.GetReportStatsAsync(period);

            var dashboardSummary = new
            {
                TotalDoctors = generalStats.NumberOfDoctors,
                TotalPatients = generalStats.NumberOfPatients,
                TotalStudies = generalStats.NumberOfStudies,
                TotalReports = reportStats.Total,
                PendingReports = reportStats.ReportGenerated, // Reports waiting to be reviewed
                ReviewedReports = reportStats.Reviewed,
                FailedReports = reportStats.Failed,
                InProgressReports = reportStats.InProgress
            };

            return new ApiResponse<object>(
                data: dashboardSummary,
                message: "Dashboard summary retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return new ApiResponse<object?>(
                data: null,
                message: "An error occurred while retrieving dashboard summary",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get study assignment statistics (assigned, reviewed, unassigned)
    /// </summary>
    /// <param name="timePeriod">Time period filter: LastWeek, LastMonth, Last3Months, Last6Months, Last9Months, LastYear, AllTime</param>
    /// <returns>Study assignment statistics</returns>
    [HttpGet("studies")]
    public async Task<IActionResult> GetStudyStatsAsync([FromQuery] string? timePeriod = null)
    {
        try
        {
            var period = ParseTimePeriod(timePeriod);
            var stats = await _statisticsService.GetStudyStatsAsync(period);

            return new ApiResponse<StudyStatsDTO>(
                data: stats,
                message: "Study statistics retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving study statistics");
            return new ApiResponse<StudyStatsDTO?>(
                data: null,
                message: "An error occurred while retrieving study statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
