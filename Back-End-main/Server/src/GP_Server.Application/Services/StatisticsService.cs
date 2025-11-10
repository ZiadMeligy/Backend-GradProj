using AutoMapper;
using GP_Server.Application.DTOs.Statistics;
using GP_Server.Application.Exceptions;
using GP_Server.Application.Interfaces;
using GP_Server.Application.Helpers;
using GP_Server.Domain.Entities;
using GP_Server.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GP_Server.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IRepository<Study> _studyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IOrthancService _orthancService;
    private readonly IMapper _mapper;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        IRepository<Study> studyRepository,
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        IOrthancService orthancService,
        IMapper mapper,
        ILogger<StatisticsService> logger)
    {
        _studyRepository = studyRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _orthancService = orthancService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GeneralStatsDTO> GetGeneralStatsAsync(TimePeriod? timePeriod = null)
    {
        try
        {
            _logger.LogInformation("Fetching general statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");

            // Get number of doctors
            var doctorRole = await _roleManager.FindByNameAsync("Doctor");
            var numberOfDoctors = 0;
            if (doctorRole != null)
            {
                var doctorsInRole = await _userManager.GetUsersInRoleAsync("Doctor");
                
                if (timePeriod.HasValue && timePeriod.Value != TimePeriod.AllTime)
                {
                    var (startDate, endDate) = TimePeriodHelper.GetDateRangeForPeriod(timePeriod.Value);
                    numberOfDoctors = doctorsInRole.Count(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);
                }
                else
                {
                    numberOfDoctors = doctorsInRole.Count;
                }
            }

            // Get number of patients from Orthanc (Note: Orthanc doesn't have date filtering, so this remains total count)
            var orthancPatients = await _orthancService.GetAllPatientsAsync();
            var numberOfPatients = orthancPatients.TotalCount;

            // Get number of studies from our database with time filtering
            IEnumerable<Study> studies;
            if (timePeriod.HasValue && timePeriod.Value != TimePeriod.AllTime)
            {
                var (startDate, endDate) = TimePeriodHelper.GetDateRangeForPeriod(timePeriod.Value);
                studies = await _studyRepository.FindAsync(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate);
            }
            else
            {
                studies = await _studyRepository.GetAllAsync();
            }
            var numberOfStudies = studies.Count();

            var stats = new GeneralStatsDTO
            {
                NumberOfDoctors = numberOfDoctors,
                NumberOfPatients = numberOfPatients,
                NumberOfStudies = numberOfStudies
            };

            _logger.LogInformation("General statistics retrieved: {DoctorCount} doctors, {PatientCount} patients, {StudyCount} studies", 
                numberOfDoctors, numberOfPatients, numberOfStudies);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching general statistics");
            throw new ServerErrorException("An error occurred while retrieving general statistics.");
        }
    }

    public async Task<ReportStatsDTO> GetReportStatsAsync(TimePeriod? timePeriod = null)
    {
        try
        {
            _logger.LogInformation("Fetching report statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");

            IEnumerable<Study> studies;
            if (timePeriod.HasValue && timePeriod.Value != TimePeriod.AllTime)
            {
                var (startDate, endDate) = TimePeriodHelper.GetDateRangeForPeriod(timePeriod.Value);
                studies = await _studyRepository.FindAsync(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate);
            }
            else
            {
                studies = await _studyRepository.GetAllAsync();
            }
            
            var stats = new ReportStatsDTO
            {
                NoReport = studies.Count(s => s.ReportStatus == ReportStatus.NoReport),
                WaitingForQueue = studies.Count(s => s.ReportStatus == ReportStatus.WaitingForQueue),
                Queued = studies.Count(s => s.ReportStatus == ReportStatus.Queued),
                InProgress = studies.Count(s => s.ReportStatus == ReportStatus.InProgress),
                ReportGenerated = studies.Count(s => s.ReportStatus == ReportStatus.ReportGenerated),
                Failed = studies.Count(s => s.ReportStatus == ReportStatus.Failed),
                Reviewed = studies.Count(s => s.ReportStatus == ReportStatus.Reviewed),
                Total = studies.Count()
            };

            _logger.LogInformation("Report statistics retrieved: Total={Total}, NoReport={NoReport}, Queued={Queued}, InProgress={InProgress}, Generated={Generated}, Failed={Failed}, Reviewed={Reviewed}", 
                stats.Total, stats.NoReport, stats.Queued, stats.InProgress, stats.ReportGenerated, stats.Failed, stats.Reviewed);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching report statistics");
            throw new ServerErrorException("An error occurred while retrieving report statistics.");
        }
    }

    public async Task<List<DoctorStatsDTO>> GetDoctorStatsAsync(TimePeriod? timePeriod = null)
    {
        try
        {
            _logger.LogInformation("Fetching doctor statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");

            // Get all doctors
            var doctorRole = await _roleManager.FindByNameAsync("Doctor");
            if (doctorRole == null)
            {
                _logger.LogWarning("Doctor role not found");
                return new List<DoctorStatsDTO>();
            }

            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            var doctorStats = new List<DoctorStatsDTO>();

            foreach (var doctor in doctors)
            {
                // Get studies assigned to this doctor with time filtering
                IEnumerable<Study> assignedStudies;
                if (timePeriod.HasValue && timePeriod.Value != TimePeriod.AllTime)
                {
                    var (startDate, endDate) = TimePeriodHelper.GetDateRangeForPeriod(timePeriod.Value);
                    assignedStudies = await _studyRepository.FindAsync(s => s.AssignedDoctorId == doctor.Id && s.CreatedAt >= startDate && s.CreatedAt <= endDate);
                }
                else
                {
                    assignedStudies = await _studyRepository.FindAsync(s => s.AssignedDoctorId == doctor.Id);
                }
                var studiesList = assignedStudies.ToList();

                var stats = new DoctorStatsDTO
                {
                    DoctorId = doctor.Id,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}".Trim(),
                    DoctorEmail = doctor.Email ?? "",
                    TotalAssignedReports = studiesList.Count,
                    FinishedReports = studiesList.Count(s => s.ReportStatus == ReportStatus.Reviewed),
                    WaitingReportsToBeReviewed = studiesList.Count(s => s.ReportStatus == ReportStatus.ReportGenerated),
                    ReviewedReports = studiesList.Count(s => s.ReportStatus == ReportStatus.Reviewed)
                };

                doctorStats.Add(stats);
            }

            _logger.LogInformation("Doctor statistics retrieved for {DoctorCount} doctors", doctorStats.Count);

            return doctorStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor statistics");
            throw new ServerErrorException("An error occurred while retrieving doctor statistics.");
        }
    }

    public async Task<DoctorStatsDTO?> GetDoctorStatsByIdAsync(string doctorId, TimePeriod? timePeriod = null)
    {
        try
        {
            _logger.LogInformation("Fetching statistics for doctor: {DoctorId} for period: {TimePeriod}", doctorId, timePeriod?.ToString() ?? "All Time");

            // Get the doctor by ID
            var doctor = await _userManager.FindByIdAsync(doctorId);
            if (doctor == null)
            {
                _logger.LogWarning("Doctor with ID {DoctorId} not found", doctorId);
                return null;
            }

            // Verify the user is actually a doctor
            var isDoctor = await _userManager.IsInRoleAsync(doctor, "Doctor");
            if (!isDoctor)
            {
                _logger.LogWarning("User with ID {DoctorId} is not in Doctor role", doctorId);
                return null;
            }

            // Get studies assigned to this doctor with time filtering
            IEnumerable<Study> assignedStudies;
            if (timePeriod.HasValue && timePeriod.Value != TimePeriod.AllTime)
            {
                var (startDate, endDate) = TimePeriodHelper.GetDateRangeForPeriod(timePeriod.Value);
                assignedStudies = await _studyRepository.FindAsync(s => s.AssignedDoctorId == doctorId && s.CreatedAt >= startDate && s.CreatedAt <= endDate);
            }
            else
            {
                assignedStudies = await _studyRepository.FindAsync(s => s.AssignedDoctorId == doctorId);
            }
            var studiesList = assignedStudies.ToList();

            var stats = new DoctorStatsDTO
            {
                DoctorId = doctor.Id,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}".Trim(),
                DoctorEmail = doctor.Email ?? "",
                TotalAssignedReports = studiesList.Count,
                FinishedReports = studiesList.Count(s => s.ReportStatus == ReportStatus.Reviewed),
                WaitingReportsToBeReviewed = studiesList.Count(s => s.ReportStatus == ReportStatus.ReportGenerated),
                ReviewedReports = studiesList.Count(s => s.ReportStatus == ReportStatus.Reviewed)
            };

            _logger.LogInformation("Statistics retrieved for doctor {DoctorId}: {TotalAssigned} total, {Finished} finished, {Waiting} waiting", 
                doctorId, stats.TotalAssignedReports, stats.FinishedReports, stats.WaitingReportsToBeReviewed);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching statistics for doctor: {DoctorId}", doctorId);
            throw new ServerErrorException("An error occurred while retrieving doctor statistics.");
        }
    }

    public async Task<StatisticsDTO> GetAllStatisticsAsync(TimePeriod? timePeriod = null)
    {
        try
        {
            _logger.LogInformation("Fetching all statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");

            var generalStats = await GetGeneralStatsAsync(timePeriod);
            var reportStats = await GetReportStatsAsync(timePeriod);
            var doctorStats = await GetDoctorStatsAsync(timePeriod);

            var allStats = new StatisticsDTO
            {
                GeneralStats = generalStats,
                ReportStats = reportStats,
                DoctorStats = doctorStats
            };

            _logger.LogInformation("All statistics retrieved successfully for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");

            return allStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");
            throw new ServerErrorException("An error occurred while retrieving all statistics.");
        }
    }

    public async Task<StudyStatsDTO> GetStudyStatsAsync(TimePeriod? timePeriod = null)
    {
        try
        {
            _logger.LogInformation("Fetching study statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");

            IEnumerable<Study> studies;
            if (timePeriod.HasValue && timePeriod.Value != TimePeriod.AllTime)
            {
                var (startDate, endDate) = TimePeriodHelper.GetDateRangeForPeriod(timePeriod.Value);
                studies = await _studyRepository.FindAsync(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate);
            }
            else
            {
                studies = await _studyRepository.GetAllAsync();
            }

            var studiesList = studies.ToList();

            var stats = new StudyStatsDTO
            {
                TotalStudies = studiesList.Count,
                AssignedStudies = studiesList.Count(s => !string.IsNullOrEmpty(s.AssignedDoctorId) && s.ReportStatus != ReportStatus.Reviewed),
                ReviewedStudies = studiesList.Count(s => s.ReportStatus == ReportStatus.Reviewed),
                UnassignedStudies = studiesList.Count(s => string.IsNullOrEmpty(s.AssignedDoctorId))
            };

            _logger.LogInformation("Study statistics retrieved: Total={Total}, Assigned={Assigned}, Reviewed={Reviewed}, Unassigned={Unassigned}", 
                stats.TotalStudies, stats.AssignedStudies, stats.ReviewedStudies, stats.UnassignedStudies);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching study statistics for period: {TimePeriod}", timePeriod?.ToString() ?? "All Time");
            throw new ServerErrorException("An error occurred while retrieving study statistics.");
        }
    }
}
