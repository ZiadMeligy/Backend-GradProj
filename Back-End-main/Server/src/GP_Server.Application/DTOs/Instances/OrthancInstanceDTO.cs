using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GP_Server.Application.DTOs.Instances;

public class OrthancInstanceDTO
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    public OrthancInstanceMainDicomTags MainDicomTags { get; set; } = new();

    [JsonPropertyName("ParentSeries")]
    public string ParentSeries { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("IndexInSeries")]
    public int? IndexInSeries { get; set; }
}

public class OrthancInstanceMainDicomTags
{
    [JsonPropertyName("ImageOrientationPatient")]
    public string? ImageOrientationPatient { get; set; }

    [JsonPropertyName("ImagePositionPatient")]
    public string? ImagePositionPatient { get; set; }

    [JsonPropertyName("InstanceCreationDate")]
    public string? InstanceCreationDate { get; set; }

    [JsonPropertyName("InstanceCreationTime")]
    public string? InstanceCreationTime { get; set; }

    [JsonPropertyName("InstanceNumber")]
    public string? InstanceNumber { get; set; }

    [JsonPropertyName("SOPInstanceUID")]
    public string? SOPInstanceUID { get; set; }

    [JsonPropertyName("SliceLocation")]
    public string? SliceLocation { get; set; }

    [JsonPropertyName("SliceThickness")]
    public string? SliceThickness { get; set; }

    [JsonPropertyName("WindowCenter")]
    public string? WindowCenter { get; set; }

    [JsonPropertyName("WindowWidth")]
    public string? WindowWidth { get; set; }

    [JsonPropertyName("Rows")]
    public string? Rows { get; set; }

    [JsonPropertyName("Columns")]
    public string? Columns { get; set; }

    [JsonPropertyName("PixelSpacing")]
    public string? PixelSpacing { get; set; }
}

public class OrthancInstanceListDTO
{
    public List<OrthancInstanceDTO> Instances { get; set; } = new();
    public int TotalCount { get; set; }
}
