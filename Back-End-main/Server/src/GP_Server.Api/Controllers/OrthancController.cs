using GP_Server.Application.ApiResponses;
using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Patients;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Application.DTOs.Series;
using GP_Server.Application.DTOs.Instances;
using GP_Server.Application.Interfaces;
using GP_Server.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GP_Server.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Add authorization if needed
public class OrthancController : ControllerBase
{
    private readonly IOrthancService _orthancService;
    private readonly IStudyService _studyService;

    public OrthancController(IOrthancService orthancService, IStudyService studyService)
    {
        _orthancService = orthancService;
        _studyService = studyService;
    }

    /// <summary>
    /// Get all patients from Orthanc PACS server
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>List of patients with pagination</returns>
    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatientsAsync([FromQuery] PaginationParameters? pagination = null)
    {
        var patients = await _orthancService.GetAllPatientsAsync(pagination);

        if (pagination != null)
        {
            return new PaginatedResponse<List<OrthancPatientDTO>>(
                data: patients.Patients,
                totalRecords: patients.TotalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                message: "Patients retrieved successfully from Orthanc server",
                statusCode: StatusCodes.Status200OK
            );
        }

        return new ApiResponse<List<OrthancPatientDTO>>(
            data: patients.Patients,
            message: "Patients retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get a specific patient by ID from Orthanc PACS server
    /// </summary>
    /// <param name="patientId">The Orthanc patient ID</param>
    /// <returns>Patient details</returns>
    [HttpGet("patients/{patientId}")]
    public async Task<IActionResult> GetPatientByIdAsync(string patientId)
    {
        var patient = await _orthancService.GetPatientByIdAsync(patientId);

        if (patient == null)
        {
            return new ApiResponse<OrthancPatientDTO?>(
                data: null,
                message: "Patient not found in Orthanc server",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return new ApiResponse<OrthancPatientDTO>(
            data: patient,
            message: "Patient retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get all studies for a specific patient from Orthanc PACS server
    /// </summary>
    /// <param name="patientId">The Orthanc patient ID</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>List of studies for the patient with pagination</returns>
    [HttpGet("patients/{patientId}/studies")]
    public async Task<IActionResult> GetPatientStudiesAsync(string patientId, [FromQuery] PaginationParameters? pagination = null)
    {
        var studies = await _orthancService.GetPatientStudiesAsync(patientId, pagination);

        if (pagination != null)
        {
            return new PaginatedResponse<List<OrthancStudyDTO>>(
                data: studies.Studies,
                totalRecords: studies.TotalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                message: "Patient studies retrieved successfully from Orthanc server",
                statusCode: StatusCodes.Status200OK
            );
        }

        return new ApiResponse<List<OrthancStudyDTO>>(
            data: studies.Studies,
            message: "Patient studies retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get a specific study by ID from Orthanc PACS server
    /// </summary>
    /// <param name="studyId">The Orthanc study ID</param>
    /// <returns>Study details</returns>
    [HttpGet("studies/{studyId}")]
    public async Task<IActionResult> GetStudyByIdAsync(string studyId)
    {
        var study = await _orthancService.GetStudyByIdAsync(studyId);

        if (study == null)
        {
            return new ApiResponse<OrthancStudyDTO?>(
                data: null,
                message: "Study not found in Orthanc server",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return new ApiResponse<OrthancStudyDTO>(
            data: study,
            message: "Study retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get all series for a specific study from Orthanc PACS server
    /// </summary>
    /// <param name="studyId">The Orthanc study ID</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>List of series for the study with pagination</returns>
    [HttpGet("studies/{studyId}/series")]
    public async Task<IActionResult> GetStudySeriesAsync(string studyId, [FromQuery] PaginationParameters? pagination = null)
    {
        var series = await _orthancService.GetStudySeriesAsync(studyId, pagination);

        if (pagination != null)
        {
            return new PaginatedResponse<List<OrthancSeriesDTO>>(
                data: series.Series,
                totalRecords: series.TotalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                message: "Study series retrieved successfully from Orthanc server",
                statusCode: StatusCodes.Status200OK
            );
        }

        return new ApiResponse<List<OrthancSeriesDTO>>(
            data: series.Series,
            message: "Study series retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get a specific series by ID from Orthanc PACS server
    /// </summary>
    /// <param name="seriesId">The Orthanc series ID</param>
    /// <returns>Series details</returns>
    [HttpGet("series/{seriesId}")]
    public async Task<IActionResult> GetSeriesByIdAsync(string seriesId)
    {
        var series = await _orthancService.GetSeriesByIdAsync(seriesId);

        if (series == null)
        {
            return new ApiResponse<OrthancSeriesDTO?>(
                data: null,
                message: "Series not found in Orthanc server",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return new ApiResponse<OrthancSeriesDTO>(
            data: series,
            message: "Series retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Check if Orthanc server is available
    /// </summary>
    /// <returns>Server availability status</returns>
    [HttpGet("health")]
    public async Task<IActionResult> CheckOrthancHealthAsync()
    {
        var isAvailable = await _orthancService.IsOrthancServerAvailableAsync();

        var status = isAvailable ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
        var message = isAvailable ? "Orthanc server is available" : "Orthanc server is not available";

        return new ApiResponse<object>(
            data: new { IsAvailable = isAvailable },
            message: message,
            statusCode: status
        );
    }

    /// <summary>
    /// Get all instances for a specific series from Orthanc PACS server
    /// </summary>
    /// <param name="seriesId">The Orthanc series ID</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>List of instances for the series with pagination</returns>
    [HttpGet("series/{seriesId}/instances")]
    public async Task<IActionResult> GetSeriesInstancesAsync(string seriesId, [FromQuery] PaginationParameters? pagination = null)
    {
        var instances = await _orthancService.GetSeriesInstancesAsync(seriesId, pagination);

        if (pagination != null)
        {
            return new PaginatedResponse<List<OrthancInstanceDTO>>(
                data: instances.Instances,
                totalRecords: instances.TotalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                message: "Series instances retrieved successfully from Orthanc server",
                statusCode: StatusCodes.Status200OK
            );
        }

        return new ApiResponse<List<OrthancInstanceDTO>>(
            data: instances.Instances,
            message: "Series instances retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get a specific instance by ID from Orthanc PACS server
    /// </summary>
    /// <param name="instanceId">The Orthanc instance ID</param>
    /// <returns>Instance details</returns>
    [HttpGet("instances/{instanceId}")]
    public async Task<IActionResult> GetInstanceByIdAsync(string instanceId)
    {
        var instance = await _orthancService.GetInstanceByIdAsync(instanceId);

        if (instance == null)
        {
            return new ApiResponse<OrthancInstanceDTO?>(
                data: null,
                message: "Instance not found in Orthanc server",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return new ApiResponse<OrthancInstanceDTO>(
    data: instance,
    message: "Instance retrieved successfully from Orthanc server",
    statusCode: StatusCodes.Status200OK
);
    }

    /// <summary>
    /// Get DICOM image preview for a specific instance
    /// </summary>
    /// <param name="instanceId">The Orthanc instance ID</param>
    /// <param name="format">Image format (png, jpg, jpeg)</param>
    /// <returns>Image file</returns>
    [HttpGet("instances/{instanceId}/image")]
    public async Task<IActionResult> GetInstanceImageAsync(string instanceId, [FromQuery] string format = "png")
    {
        var imageBytes = await _orthancService.GetInstanceImageAsync(instanceId, format);

        var contentType = format.ToLower() switch
        {
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            _ => "image/png"
        };

        return File(imageBytes, contentType);
    }

    /// <summary>
    /// Get raw DICOM file for a specific instance
    /// </summary>
    /// <param name="instanceId">The Orthanc instance ID</param>
    /// <returns>DICOM file</returns>
    [HttpGet("instances/{instanceId}/dicom")]
    public async Task<IActionResult> GetInstanceDicomFileAsync(string instanceId)
    {
        var dicomBytes = await _orthancService.GetInstanceDicomFileAsync(instanceId);
        return File(dicomBytes, "application/dicom", $"{instanceId}.dcm");
    }

    /// <summary>
    /// Get DICOM tags (metadata) for a specific instance
    /// </summary>
    /// <param name="instanceId">The Orthanc instance ID</param>
    /// <returns>DICOM tags as JSON</returns>
    [HttpGet("instances/{instanceId}/tags")]
    public async Task<IActionResult> GetInstanceTagsAsync(string instanceId)
    {
        var tagsJson = await _orthancService.GetInstanceTagsAsync(instanceId);
        return Content(tagsJson, "application/json");
    }

    /// <summary>
    /// Get all DICOM data for a study (for DICOM viewer)
    /// This endpoint combines study metadata with all series and instances
    /// </summary>
    /// <param name="studyId">The Orthanc study ID</param>
    /// <returns>Complete study data for DICOM viewer</returns>
    [HttpGet("studies/{studyId}/viewer-data")]
    public async Task<IActionResult> GetStudyViewerDataAsync(string studyId)
    {
        // Get study details
        var study = await _orthancService.GetStudyByIdAsync(studyId);
        if (study == null)
        {
            return new ApiResponse<object?>(
                data: null,
                message: "Study not found in Orthanc server",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        // Get all series for this study
        var seriesData = await _orthancService.GetStudySeriesAsync(studyId);

        // For each series, get all instances
        var viewerData = new
        {
            Study = study,
            Series = new List<object>()
        };

        var seriesList = new List<object>();

        foreach (var series in seriesData.Series)
        {
            var instancesData = await _orthancService.GetSeriesInstancesAsync(series.Id);

            seriesList.Add(new
            {
                SeriesInfo = series,
                Instances = instancesData.Instances.Select(instance => new
                {
                    InstanceInfo = instance,
                    ImageUrl = $"/api/orthanc/instances/{instance.Id}/image",
                    DicomUrl = $"/api/orthanc/instances/{instance.Id}/dicom",
                    TagsUrl = $"/api/orthanc/instances/{instance.Id}/tags"
                }).OrderBy(i => i.InstanceInfo.MainDicomTags.InstanceNumber).ToList()
            });
        }

        return new ApiResponse<object>(
            data: new { Study = study, Series = seriesList },
            message: "Study viewer data retrieved successfully from Orthanc server",
                statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Upload a DICOM file to Orthanc PACS server
    /// </summary>
    /// <param name="request">Upload request containing the DICOM file</param>
    /// <returns>Upload response with Orthanc IDs and metadata</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDicomFileAsync([FromForm] UploadDicomRequest request)
    {
        // use NameIdentifier claim to get user ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return new ApiResponse<object>(
                data: new { },
                message: "User ID not found in token",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        
        var uploadResponse = await _orthancService.UploadDicomFileAsync(request.DicomFile, userId);

        // Queue for report generation if requested
        if (request.GenerateReport)
        {
            await _orthancService.QueueForReportGenerationAsync(uploadResponse.OrthancInstanceId);
        }

        return new ApiResponse<UploadDicomResponse>(
        data: uploadResponse,
        message: "DICOM file uploaded successfully to Orthanc server",
        statusCode: StatusCodes.Status201Created
    );
    }

    /// <summary>
    /// Upload multiple DICOM files to Orthanc PACS server
    /// </summary>
    /// <param name="request">Bulk upload request containing multiple DICOM files</param>
    /// <returns>Bulk upload response with results for each file</returns>
    [HttpPost("bulk-upload")]
    public async Task<IActionResult> BulkUploadDicomFilesAsync([FromForm] BulkUploadDicomRequest request)
    {
        var bulkUploadResponse = await _orthancService.BulkUploadDicomFilesAsync(request.DicomFiles, request.GenerateReport);

        var statusCode = bulkUploadResponse.FailedCount == 0 ?
            StatusCodes.Status201Created :
            StatusCodes.Status207MultiStatus;

        return new ApiResponse<BulkUploadDicomResponse>(
            data: bulkUploadResponse,
            message: bulkUploadResponse.Message,
            statusCode: statusCode
        );
    }

    /// <summary>
    /// Get all studies with their associated patient information and status from Orthanc PACS server
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>List of studies with patient information, status and pagination</returns>
    [HttpGet("studies")]
    public async Task<IActionResult> GetAllStudiesWithPatientsAsync([FromQuery] PaginationParameters? pagination = null)
    {
        // Get all studies
        var studiesResult = await _orthancService.GetAllStudiesAsync();
        var studies = studiesResult.Studies;
        var totalCount = studies.Count;

        // For each study, get the patient info and status
        var result = new List<object>();
        foreach (var study in studies)
        {
            var patient = await _orthancService.GetPatientByIdAsync(study.ParentPatient);
            var status = await _studyService.GetStudyStatusAsync(study.Id);

            result.Add(new
            {
                Study = study,
                Patient = patient,
                Status = status
            });
        }

        // Order by CreatedAt descending (from Status object)
        result = result.OrderByDescending(x => ((dynamic)x).Status?.CreatedAt ?? DateTime.MinValue).ToList();

        // Apply pagination after ordering
        var finalResult = result;
        if (pagination != null)
        {
            var skip = (pagination.PageNumber - 1) * pagination.PageSize;
            finalResult = result.Skip(skip).Take(pagination.PageSize).ToList();
        }

        if (pagination != null)
        {
            return new PaginatedResponse<List<object>>(
                data: finalResult,
                totalRecords: totalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                message: "Studies with patient information and status retrieved successfully from Orthanc server",
                statusCode: StatusCodes.Status200OK
            );
        }

        return new ApiResponse<List<object>>(
            data: finalResult,
            message: "Studies with patient information and status retrieved successfully from Orthanc server",
            statusCode: StatusCodes.Status200OK
        );
    }


    /// <summary>
    /// Get study status by Orthanc study ID
    /// </summary>
    /// <param name="studyId">The Orthanc study ID</param>
    /// <returns>Study status information</returns>
    [HttpGet("studies/{studyId}/status")]
    public async Task<IActionResult> GetStudyStatusAsync(string studyId)
    {
        var studyStatus = await _studyService.GetStudyStatusAsync(studyId);

        if (studyStatus == null)
        {
            return new ApiResponse<StudyStatusDTO?>(
                data: null,
                message: "Study status not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return new ApiResponse<StudyStatusDTO>(
            data: studyStatus,
            message: "Study status retrieved successfully",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get all studies with their status
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="statusFilter">Filter by report status</param>
    /// <returns>List of studies with status information</returns>
    [HttpGet("studies-with-status")]
    public async Task<IActionResult> GetStudiesWithStatusAsync(
        [FromQuery] PaginationParameters? pagination = null,
        [FromQuery] ReportStatus? statusFilter = null)
    {
        var pageNumber = pagination?.PageNumber ?? 1;
        var pageSize = pagination?.PageSize ?? 50;

        var studiesWithStatus = await _studyService.GetAllStudiesWithStatusAsync(pageNumber, pageSize, statusFilter);

        if (pagination != null)
        {
            return new PaginatedResponse<List<StudyStatusDTO>>(
                data: studiesWithStatus.Studies,
                totalRecords: studiesWithStatus.TotalCount,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                message: "Studies with status retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }

        return new ApiResponse<List<StudyStatusDTO>>(
            data: studiesWithStatus.Studies,
            message: "Studies with status retrieved successfully",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Get studies by report status
    /// </summary>
    /// <param name="status">The report status to filter by</param>
    /// <returns>List of studies with the specified status</returns>
    [HttpGet("studies/by-status/{status}")]
    public async Task<IActionResult> GetStudiesByStatusAsync(ReportStatus status)
    {
        var studies = await _studyService.GetStudiesByStatusAsync(status);

        return new ApiResponse<List<StudyStatusDTO>>(
            data: studies,
            message: $"Studies with status '{status}' retrieved successfully",
            statusCode: StatusCodes.Status200OK
        );
    }

    [HttpPost("studies/{studyId}/assign-doctor/{doctorId}")]
    public async Task<IActionResult> AssignStudyToDoctorAsync(string studyId, string doctorId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(studyId) || string.IsNullOrWhiteSpace(doctorId))
        {
            return BadRequest("Study ID and Doctor ID are required.");
        }

        // Assign the study to the doctor
        await _studyService.AssignStudyToDoctorAsync(studyId, doctorId);

        return new ApiResponse<object>(
            data: new { StudyId = studyId, DoctorId = doctorId },
            message: $"Study '{studyId}' has been successfully assigned to Doctor '{doctorId}'.",
            statusCode: StatusCodes.Status200OK
        );
    }

    /// <summary>
    /// Queue a study for report generation
    /// </summary>
    /// <param name="studyId">The ID of the study to queue for report generation</param>
    /// <returns>Success response when study is queued</returns>
    [HttpPost("studies/{studyId}/queue-report")]
    public async Task<IActionResult> QueueStudyForReportGenerationAsync(string studyId)
    {
        // Get user ID from claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return new ApiResponse<object>(
                data: new { },
                message: "User ID not found in token",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        await _orthancService.QueueStudyForReportGenerationAsync(studyId, userId);

        return new ApiResponse<object>(
            data: new { StudyId = studyId },
            message: "Study successfully queued for report generation",
            statusCode: StatusCodes.Status200OK
        );
    }
}
