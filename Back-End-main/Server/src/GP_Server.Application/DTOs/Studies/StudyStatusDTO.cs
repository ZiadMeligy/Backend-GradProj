using System.ComponentModel.DataAnnotations;
using GP_Server.Domain.Entities;

namespace GP_Server.Application.DTOs.Studies;

public class StudyStatusDTO
{
    public Guid Id { get; set; }
    public string OrthancStudyId { get; set; } = string.Empty;
    public string? StudyInstanceUID { get; set; }
    public string? StudyDescription { get; set; }
    public string? StudyDate { get; set; }
    public string? PatientId { get; set; }
    public string? PatientName { get; set; }
    public ReportStatus ReportStatus { get; set; }
    public DateTime? ReportQueuedAt { get; set; }
    public DateTime? ReportGeneratedAt { get; set; }
    public string? GeneratedReportInstanceId { get; set; }
    public string? ReportGenerationError { get; set; }
    public int ReportGenerationAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string? AssignedDoctorId { get; set; }
}

public class CreateStudyDTO
{
    [Required]
    public string OrthancStudyId { get; set; } = string.Empty;
    public string? StudyInstanceUID { get; set; }
    public string? StudyDescription { get; set; }
    public string? StudyDate { get; set; }
    public string? PatientId { get; set; }
    public string? PatientName { get; set; }
}

public class StudyWithStatusListDTO
{
    public List<StudyStatusDTO> Studies { get; set; } = new();
    public int TotalCount { get; set; }
}
