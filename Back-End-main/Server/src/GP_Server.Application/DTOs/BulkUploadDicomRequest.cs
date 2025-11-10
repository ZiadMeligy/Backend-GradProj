using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GP_Server.Application.DTOs;

public class BulkUploadDicomRequest
{
    [Required(ErrorMessage = "At least one DICOM file is required")]
    public List<IFormFile> DicomFiles { get; set; } = new();
    public bool GenerateReport { get; set; } = false;
}

public class BulkUploadDicomResponse
{
    public List<UploadDicomResponse> SuccessfulUploads { get; set; } = new();
    public List<FailedUpload> FailedUploads { get; set; } = new();
    public int TotalFiles { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class FailedUpload
{
    public string FileName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
