using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GP_Server.Application.DTOs.Series;

public class OrthancSeriesDTO
{
    [JsonPropertyName("ID")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("MainDicomTags")]
    public OrthancSeriesMainDicomTags MainDicomTags { get; set; } = new();

    [JsonPropertyName("ParentStudy")]
    public string ParentStudy { get; set; } = string.Empty;

    [JsonPropertyName("Instances")]
    public List<string> Instances { get; set; } = new();

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;
}

public class OrthancSeriesMainDicomTags
{
    [JsonPropertyName("BodyPartExamined")]
    public string? BodyPartExamined { get; set; }

    [JsonPropertyName("Modality")]
    public string? Modality { get; set; }

    [JsonPropertyName("ProtocolName")]
    public string? ProtocolName { get; set; }

    [JsonPropertyName("SeriesDescription")]
    public string? SeriesDescription { get; set; }

    [JsonPropertyName("SeriesInstanceUID")]
    public string? SeriesInstanceUID { get; set; }

    [JsonPropertyName("SeriesNumber")]
    public string? SeriesNumber { get; set; }

    [JsonPropertyName("SeriesDate")]
    public string? SeriesDate { get; set; }

    [JsonPropertyName("SeriesTime")]
    public string? SeriesTime { get; set; }
}

public class OrthancSeriesListDTO
{
    public List<OrthancSeriesDTO> Series { get; set; } = new();
    public int TotalCount { get; set; }
}
