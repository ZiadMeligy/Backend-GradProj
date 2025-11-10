using GP_Server.Application.DTOs.Studies;
using GP_Server.Domain.Entities;

namespace GP_Server.Application.Interfaces;

public interface IStudyService
{
    /// <summary>
    /// Create or update a study record based on Orthanc data
    /// </summary>
    Task<StudyStatusDTO> CreateOrUpdateStudyAsync(OrthancStudyDTO orthancStudy, string userId);

    /// <summary>
    /// Get study status by Orthanc Study ID
    /// </summary>
    Task<StudyStatusDTO?> GetStudyStatusAsync(string orthancStudyId);

    /// <summary>
    /// Get all studies with their status
    /// </summary>
    Task<StudyWithStatusListDTO> GetAllStudiesWithStatusAsync(int pageNumber = 1, int pageSize = 50, ReportStatus? statusFilter = null);

    /// <summary>
    /// Queue study for report generation
    /// </summary>
    Task<StudyStatusDTO> QueueStudyForReportAsync(string orthancStudyId);

    /// <summary>
    /// Mark study report generation as in progress
    /// </summary>
    Task<StudyStatusDTO> MarkReportAsInProgressAsync(string orthancStudyId);

    /// <summary>
    /// Mark study report as generated
    /// </summary>
    Task<StudyStatusDTO> MarkReportAsGeneratedAsync(string orthancStudyId, string reportInstanceId);

    /// <summary>
    /// Mark study report generation as failed
    /// </summary>
    Task<StudyStatusDTO> MarkReportAsFailedAsync(string orthancStudyId, string errorMessage);

    /// <summary>
    /// Get studies by status
    /// </summary>
    Task<List<StudyStatusDTO>> GetStudiesByStatusAsync(ReportStatus status);
    Task AssignStudyToDoctorAsync(string orthancStudyId, string doctorId);
}
