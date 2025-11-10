using GP_Server.Application.DTOs.Statistics;

namespace GP_Server.Application.Interfaces;

public interface IStatisticsService
{
    /// <summary>
    /// Get general statistics including number of doctors, patients, and studies
    /// </summary>
    Task<GeneralStatsDTO> GetGeneralStatsAsync(TimePeriod? timePeriod = null);

    /// <summary>
    /// Get report statistics showing count for each report status
    /// </summary>
    Task<ReportStatsDTO> GetReportStatsAsync(TimePeriod? timePeriod = null);

    /// <summary>
    /// Get doctor statistics showing assigned, finished, and waiting reports for each doctor
    /// </summary>
    Task<List<DoctorStatsDTO>> GetDoctorStatsAsync(TimePeriod? timePeriod = null);

    /// <summary>
    /// Get statistics for a specific doctor by their ID
    /// </summary>
    Task<DoctorStatsDTO?> GetDoctorStatsByIdAsync(string doctorId, TimePeriod? timePeriod = null);

    /// <summary>
    /// Get all statistics in one call
    /// </summary>
    Task<StatisticsDTO> GetAllStatisticsAsync(TimePeriod? timePeriod = null);

    /// <summary>
    /// Get study assignment statistics (assigned, reviewed, unassigned)
    /// </summary>
    Task<StudyStatsDTO> GetStudyStatsAsync(TimePeriod? timePeriod = null);
}
