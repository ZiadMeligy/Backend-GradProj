using System.Text.Json.Serialization;

namespace GP_Server.Application.DTOs.Patients;

public class OrthancPatientDTO
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    public OrthancPatientMainDicomTags MainDicomTags { get; set; } = new();

    [JsonPropertyName("Studies")]
    public List<string> Studies { get; set; } = new();

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;
}

public class OrthancPatientMainDicomTags
{
    [JsonPropertyName("PatientBirthDate")]
    public string? PatientBirthDate { get; set; }

    [JsonPropertyName("PatientID")]
    public string? PatientID { get; set; }

    [JsonPropertyName("PatientName")]
    public string? PatientName { get; set; }

    [JsonPropertyName("PatientSex")]
    public string? PatientSex { get; set; }
}

public class OrthancPatientListDTO
{
    public List<OrthancPatientDTO> Patients { get; set; } = new();
    public int TotalCount { get; set; }
}
