using System;

namespace GP_Server.Application.DTOs.Reports;

public class ReviewReportDTO
{
    public string? OrthancId { get; set; }
    public string Findings { get; set; } = string.Empty;
    public string Impressions { get; set; } = string.Empty;
}
