using GP_Server.Domain.Entities;

namespace GP_Server.Application.DTOs.Statistics;

public class ReportStatsDTO
{
    public int NoReport { get; set; }
    public int WaitingForQueue { get; set; }
    public int Queued { get; set; }
    public int InProgress { get; set; }
    public int ReportGenerated { get; set; }
    public int Failed { get; set; }
    public int Reviewed { get; set; }
    public int Total { get; set; }
}

public class ReportStatusCountDTO
{
    public ReportStatus Status { get; set; }
    public int Count { get; set; }
}
