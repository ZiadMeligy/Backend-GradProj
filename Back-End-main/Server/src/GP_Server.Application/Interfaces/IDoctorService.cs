using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Reports;
using GP_Server.Application.DTOs.Studies;

namespace GP_Server.Application.Interfaces;

public interface IDoctorService
{
    /// <summary>
    /// Get all studies assigned to a specific doctor
    /// </summary>
    /// <param name="doctorId">The ID of the doctor</param>
    /// <returns>List of studies assigned to the doctor</returns>
    Task<List<StudyStatusDTO>> GetStudiesAssignedToDoctorAsync(string doctorId);

    /// <summary>
    /// Get all studies assigned to a specific doctor with full Orthanc data (patient, study, status)
    /// </summary>
    /// <param name="doctorId">The ID of the doctor</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>List of studies with full patient information and status</returns>
    Task<(List<object> Studies, int TotalCount)> GetStudiesAssignedToDoctorWithFullDataAsync(string doctorId, PaginationParameters? pagination = null);

    /// <summary>
    /// Review a report for a study and return updated study information
    /// </summary>
    /// <param name="studyId">The ID of the study</param>
    /// <param name="doctorId">The ID of the doctor reviewing the report</param>
    /// <param name="reviewReportDto">The review data</param>
    /// <returns>Updated study information after review</returns>
    Task<StudyStatusDTO> ReviewReportAsync(string studyId, string doctorId, ReviewReportDTO reviewReportDto);
}
