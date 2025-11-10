using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Application.DTOs;

public class UploadDicomRequest
{
    [Required(ErrorMessage = "DICOM file is required")]
    public IFormFile DicomFile { get; set; } = null!;
    public bool GenerateReport { get; set; } = false;
}

public class UploadDicomResponse
{
    public string OrthancInstanceId { get; set; } = string.Empty;
    public string OrthancPatientId { get; set; } = string.Empty;
    public string OrthancStudyId { get; set; } = string.Empty;
    public string OrthancSeriesId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string StudyDescription { get; set; } = string.Empty;
    public string Modality { get; set; } = string.Empty;
    public string StudyDate { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

