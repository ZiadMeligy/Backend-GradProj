using System.Text.Json.Serialization;

namespace GP_Server.Application.DTOs.Studies;

public class OrthancStudyDTO
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    public OrthancStudyMainDicomTags MainDicomTags { get; set; } = new();

    [JsonPropertyName("ParentPatient")]
    public string ParentPatient { get; set; } = string.Empty;

    [JsonPropertyName("Series")]
    public List<string> Series { get; set; } = new();

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;
}

public class OrthancStudyMainDicomTags
{
    [JsonPropertyName("AccessionNumber")]
    public string? AccessionNumber { get; set; }

    [JsonPropertyName("StudyDate")]
    public string? StudyDate { get; set; }

    [JsonPropertyName("StudyDescription")]
    public string? StudyDescription { get; set; }

    [JsonPropertyName("StudyID")]
    public string? StudyID { get; set; }

    [JsonPropertyName("StudyInstanceUID")]
    public string? StudyInstanceUID { get; set; }

    [JsonPropertyName("StudyTime")]
    public string? StudyTime { get; set; }
}

public class OrthancStudyListDTO
{
    public List<OrthancStudyDTO> Studies { get; set; } = new();
    public int TotalCount { get; set; }
}
