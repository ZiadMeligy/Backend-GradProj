using System;

namespace GP_Server.Application.DTOs.Statistics;

public class DoctorStatsDTO
{
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorEmail { get; set; } = string.Empty;
    public int TotalAssignedReports { get; set; }
    public int FinishedReports { get; set; }
    public int WaitingReportsToBeReviewed { get; set; }
    public int ReviewedReports { get; set; }
}
