using System;

namespace GP_Server.Application.DTOs.Statistics;

public class StatisticsDTO
{
    public GeneralStatsDTO GeneralStats { get; set; } = new();
    public ReportStatsDTO ReportStats { get; set; } = new();
    public List<DoctorStatsDTO> DoctorStats { get; set; } = new();
}
