namespace GP_Server.Application.DTOs.Statistics;

public class StudyStatsDTO
{
    /// <summary>
    /// Studies that are assigned to doctors but not yet reviewed
    /// </summary>
    public int AssignedStudies { get; set; }
    
    /// <summary>
    /// Studies that have been reviewed by doctors
    /// </summary>
    public int ReviewedStudies { get; set; }
    
    /// <summary>
    /// Studies that are not assigned to any doctor
    /// </summary>
    public int UnassignedStudies { get; set; }
    
    /// <summary>
    /// Total number of studies
    /// </summary>
    public int TotalStudies { get; set; }
}
