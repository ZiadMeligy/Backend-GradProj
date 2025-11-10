using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Patients;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Application.DTOs.Series;
using GP_Server.Application.DTOs.Instances;
using Microsoft.AspNetCore.Http;

namespace GP_Server.Application.Interfaces;

public interface IOrthancService
{
    Task<OrthancPatientListDTO> GetAllPatientsAsync(PaginationParameters? pagination = null);
    Task<OrthancPatientDTO?> GetPatientByIdAsync(string patientId);
    Task<OrthancStudyListDTO> GetPatientStudiesAsync(string patientId, PaginationParameters? pagination = null);
    Task<OrthancStudyDTO?> GetStudyByIdAsync(string studyId);
    Task<OrthancSeriesListDTO> GetStudySeriesAsync(string studyId, PaginationParameters? pagination = null);
    Task<OrthancSeriesDTO?> GetSeriesByIdAsync(string seriesId);
    Task<OrthancInstanceListDTO> GetSeriesInstancesAsync(string seriesId, PaginationParameters? pagination = null);
    Task<OrthancInstanceDTO?> GetInstanceByIdAsync(string instanceId);
    Task<byte[]> GetInstanceImageAsync(string instanceId, string format = "png");
    Task<byte[]> GetInstanceDicomFileAsync(string instanceId);
    Task<string> GetInstanceTagsAsync(string instanceId);
    Task<UploadDicomResponse> UploadDicomFileAsync(IFormFile dicomFile, string userId);
    Task<BulkUploadDicomResponse> BulkUploadDicomFilesAsync(List<IFormFile> dicomFiles, bool generateReport = false);
    Task<bool> IsOrthancServerAvailableAsync();
    Task<OrthancStudyListDTO> GetAllStudiesAsync();
    Task QueueForReportGenerationAsync(string instanceId);
    Task QueueStudyForReportGenerationAsync(string studyId,string userId);
    Task<string> UploadDicomStructuredReportAsync(byte[] dicomSrBytes);
    Task DeleteInstanceAsync(string instanceId);
}
