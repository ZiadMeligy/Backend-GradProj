using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP_Server.Domain.Entities;

public class Study : BaseEntity
{
    /// <summary>
    /// The Orthanc Study ID (unique identifier from Orthanc)
    /// </summary>
    [Required]
    public string OrthancStudyId { get; set; } = string.Empty;

    /// <summary>
    /// The DICOM Study Instance UID
    /// </summary>
    public string? StudyInstanceUID { get; set; }

    /// <summary>
    /// Study description from DICOM metadata
    /// </summary>
    public string? StudyDescription { get; set; }

    /// <summary>
    /// Study date from DICOM metadata
    /// </summary>
    public string? StudyDate { get; set; }

    /// <summary>
    /// Patient ID from DICOM metadata
    /// </summary>
    public string? PatientId { get; set; }

    /// <summary>
    /// Patient name from DICOM metadata
    /// </summary>
    public string? PatientName { get; set; }

    /// <summary>
    /// Current report generation status
    /// </summary>
    [Required]
    public ReportStatus ReportStatus { get; set; } = ReportStatus.NoReport;

    /// <summary>
    /// Date when report generation was queued
    /// </summary>
    public DateTime? ReportQueuedAt { get; set; }

    /// <summary>
    /// Date when report generation was completed
    /// </summary>
    public DateTime? ReportGeneratedAt { get; set; }

    /// <summary>
    /// The Orthanc Instance ID of the generated report (SR)
    /// </summary>
    public string? GeneratedReportInstanceId { get; set; }

    /// <summary>
    /// Error message if report generation failed
    /// </summary>
    public string? ReportGenerationError { get; set; }

    /// <summary>
    /// Number of report generation attempts
    /// </summary>
    public int ReportGenerationAttempts { get; set; } = 0;
    
    [ForeignKey(nameof(ApplicationUser))]
    public string? AssignedDoctorId { get; set; }

    public virtual ApplicationUser? AssignedDoctor { get; set; }
    
}

public enum ReportStatus
{
    /// <summary>
    /// No report has been requested or generated
    /// </summary>
    NoReport = 0,

    /// <summary>
    /// Report generation has been requested and is waiting for queue processing
    /// </summary>
    WaitingForQueue = 1,

    /// <summary>
    /// Report generation has been queued but not started
    /// </summary>
    Queued = 2,

    /// <summary>
    /// Report generation is currently in progress
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// Report has been successfully generated
    /// </summary>
    ReportGenerated = 4,

    /// <summary>
    /// Report generation failed
    /// </summary>
    Failed = 5,
    
    /// <summary>
    /// Report is Reviewed by doctor
    /// </summary>
    Reviewed = 6
}
