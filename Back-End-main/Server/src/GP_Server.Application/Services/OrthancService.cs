using System.Net.Http.Json;
using System.Text.Json;
using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Patients;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Application.DTOs.Series;
using GP_Server.Application.DTOs.Instances;
using GP_Server.Application.Exceptions;
using GP_Server.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace GP_Server.Application.Services;

public class OrthancService : IOrthancService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrthancService> _logger;
    private readonly IStudyService _studyService;
    private readonly string _orthancBaseUrl;

    public OrthancService(HttpClient httpClient, IConfiguration configuration, ILogger<OrthancService> logger, IStudyService studyService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _studyService = studyService;
        _orthancBaseUrl = _configuration["Orthanc:BaseUrl"] ?? "http://localhost:8042";
        
        // Configure basic authentication if credentials are provided
        var username = _configuration["Orthanc:Username"];
        var password = _configuration["Orthanc:Password"];
        
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }
    }

    public async Task<OrthancPatientListDTO> GetAllPatientsAsync(PaginationParameters? pagination = null)
    {
        try
        {
            _logger.LogInformation("Fetching patients from Orthanc server at {BaseUrl}", _orthancBaseUrl);

            // First, get the list of patient IDs
            var patientIdsResponse = await _httpClient.GetAsync($"{_orthancBaseUrl}/patients");
            
            if (!patientIdsResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch patient IDs from Orthanc. Status: {StatusCode}", patientIdsResponse.StatusCode);
                throw new ServerErrorException($"Failed to fetch patients from Orthanc server. Status: {patientIdsResponse.StatusCode}");
            }

            var patientIds = await patientIdsResponse.Content.ReadFromJsonAsync<List<string>>();
            
            if (patientIds == null || !patientIds.Any())
            {
                _logger.LogInformation("No patients found in Orthanc server");
                return new OrthancPatientListDTO { Patients = new List<OrthancPatientDTO>(), TotalCount = 0 };
            }

            // Apply pagination to patient IDs
            var totalCount = patientIds.Count;
            var paginatedIds = patientIds;

            if (pagination != null)
            {
                var skip = (pagination.PageNumber - 1) * pagination.PageSize;
                paginatedIds = patientIds.Skip(skip).Take(pagination.PageSize).ToList();
            }

            // Fetch detailed information for each patient
            var patients = new List<OrthancPatientDTO>();
            
            foreach (var patientId in paginatedIds)
            {
                try
                {
                    var patientResponse = await _httpClient.GetAsync($"{_orthancBaseUrl}/patients/{patientId}");
                    
                    if (patientResponse.IsSuccessStatusCode)
                    {
                        var patientJson = await patientResponse.Content.ReadAsStringAsync();
                        var patient = JsonSerializer.Deserialize<OrthancPatientDTO>(patientJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (patient != null)
                        {
                            patients.Add(patient);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to fetch patient details for ID: {PatientId}. Status: {StatusCode}", 
                            patientId, patientResponse.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching patient details for ID: {PatientId}", patientId);
                }
            }

            _logger.LogInformation("Successfully fetched {Count} patients from Orthanc", patients.Count);

            return new OrthancPatientListDTO
            {
                Patients = patients,
                TotalCount = totalCount
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while connecting to Orthanc server");
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while connecting to Orthanc server");
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching patients from Orthanc");
            throw new ServerErrorException("An unexpected error occurred while fetching patients.");
        }
    }

    public async Task<OrthancPatientDTO?> GetPatientByIdAsync(string patientId)
    {
        try
        {
            _logger.LogInformation("Fetching patient {PatientId} from Orthanc server", patientId);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/patients/{patientId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Patient {PatientId} not found in Orthanc server", patientId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch patient {PatientId} from Orthanc. Status: {StatusCode}", 
                    patientId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch patient from Orthanc server. Status: {response.StatusCode}");
            }

            var patientJson = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<OrthancPatientDTO>(patientJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully fetched patient {PatientId} from Orthanc", patientId);
            return patient;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching patient {PatientId} from Orthanc server", patientId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching patient {PatientId} from Orthanc server", patientId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching patient {PatientId} from Orthanc", patientId);
            throw new ServerErrorException("An unexpected error occurred while fetching the patient.");
        }
    }

    public async Task<bool> IsOrthancServerAvailableAsync()
    {
        try
        {
            _logger.LogInformation("Checking Orthanc server availability at {BaseUrl}", _orthancBaseUrl);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/system");
            var isAvailable = response.IsSuccessStatusCode;

            _logger.LogInformation("Orthanc server availability check result: {IsAvailable}", isAvailable);
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Orthanc server availability");
            return false;
        }
    }

    public async Task<OrthancStudyListDTO> GetPatientStudiesAsync(string patientId, PaginationParameters? pagination = null)
    {
        try
        {
            _logger.LogInformation("Fetching studies for patient {PatientId} from Orthanc server", patientId);

            // First, get the patient to verify it exists and get the studies list
            var patient = await GetPatientByIdAsync(patientId);
            if (patient == null)
            {
                _logger.LogWarning("Patient {PatientId} not found in Orthanc server", patientId);
                return new OrthancStudyListDTO { Studies = new List<OrthancStudyDTO>(), TotalCount = 0 };
            }

            if (patient.Studies == null || !patient.Studies.Any())
            {
                _logger.LogInformation("No studies found for patient {PatientId}", patientId);
                return new OrthancStudyListDTO { Studies = new List<OrthancStudyDTO>(), TotalCount = 0 };
            }

            // Apply pagination to study IDs
            var totalCount = patient.Studies.Count;
            var paginatedStudyIds = patient.Studies;

            if (pagination != null)
            {
                var skip = (pagination.PageNumber - 1) * pagination.PageSize;
                paginatedStudyIds = patient.Studies.Skip(skip).Take(pagination.PageSize).ToList();
            }

            // Fetch detailed information for each study
            var studies = new List<OrthancStudyDTO>();

            foreach (var studyId in paginatedStudyIds)
            {
                try
                {
                    var study = await GetStudyByIdAsync(studyId);
                    if (study != null)
                    {
                        studies.Add(study);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching study details for ID: {StudyId}", studyId);
                }
            }

            _logger.LogInformation("Successfully fetched {Count} studies for patient {PatientId}", studies.Count, patientId);

            return new OrthancStudyListDTO
            {
                Studies = studies,
                TotalCount = totalCount
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching studies for patient {PatientId}", patientId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching studies for patient {PatientId}", patientId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching studies for patient {PatientId}", patientId);
            throw new ServerErrorException("An unexpected error occurred while fetching patient studies.");
        }
    }

    public async Task<OrthancStudyDTO?> GetStudyByIdAsync(string studyId)
    {
        try
        {
            _logger.LogInformation("Fetching study {StudyId} from Orthanc server", studyId);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/studies/{studyId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Study {StudyId} not found in Orthanc server", studyId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch study {StudyId} from Orthanc. Status: {StatusCode}",
                    studyId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch study from Orthanc server. Status: {response.StatusCode}");
            }

            var studyJson = await response.Content.ReadAsStringAsync();
            var study = JsonSerializer.Deserialize<OrthancStudyDTO>(studyJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully fetched study {StudyId} from Orthanc", studyId);
            return study;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching study {StudyId} from Orthanc server", studyId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching study {StudyId} from Orthanc server", studyId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching study {StudyId} from Orthanc", studyId);
            throw new ServerErrorException("An unexpected error occurred while fetching the study.");
        }
    }

    public async Task<OrthancSeriesListDTO> GetStudySeriesAsync(string studyId, PaginationParameters? pagination = null)
    {
        try
        {
            _logger.LogInformation("Fetching series for study {StudyId} from Orthanc server", studyId);

            // First, get the study to verify it exists and get the series list
            var study = await GetStudyByIdAsync(studyId);
            if (study == null)
            {
                _logger.LogWarning("Study {StudyId} not found in Orthanc server", studyId);
                return new OrthancSeriesListDTO { Series = new List<OrthancSeriesDTO>(), TotalCount = 0 };
            }

            if (study.Series == null || !study.Series.Any())
            {
                _logger.LogInformation("No series found for study {StudyId}", studyId);
                return new OrthancSeriesListDTO { Series = new List<OrthancSeriesDTO>(), TotalCount = 0 };
            }

            // Apply pagination to series IDs
            var totalCount = study.Series.Count;
            var paginatedSeriesIds = study.Series;

            if (pagination != null)
            {
                var skip = (pagination.PageNumber - 1) * pagination.PageSize;
                paginatedSeriesIds = study.Series.Skip(skip).Take(pagination.PageSize).ToList();
            }

            // Fetch detailed information for each series
            var seriesList = new List<OrthancSeriesDTO>();

            foreach (var seriesId in paginatedSeriesIds)
            {
                try
                {
                    var series = await GetSeriesByIdAsync(seriesId);
                    if (series != null)
                    {
                        seriesList.Add(series);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching series details for ID: {SeriesId}", seriesId);
                }
            }

            _logger.LogInformation("Successfully fetched {Count} series for study {StudyId}", seriesList.Count, studyId);

            return new OrthancSeriesListDTO
            {
                Series = seriesList,
                TotalCount = totalCount
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching series for study {StudyId}", studyId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching series for study {StudyId}", studyId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching series for study {StudyId}", studyId);
            throw new ServerErrorException("An unexpected error occurred while fetching study series.");
        }
    }

    public async Task<OrthancSeriesDTO?> GetSeriesByIdAsync(string seriesId)
    {
        try
        {
            _logger.LogInformation("Fetching series {SeriesId} from Orthanc server", seriesId);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/series/{seriesId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Series {SeriesId} not found in Orthanc server", seriesId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch series {SeriesId} from Orthanc. Status: {StatusCode}",
                    seriesId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch series from Orthanc server. Status: {response.StatusCode}");
            }

            var seriesJson = await response.Content.ReadAsStringAsync();
            var series = JsonSerializer.Deserialize<OrthancSeriesDTO>(seriesJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully fetched series {SeriesId} from Orthanc", seriesId);
                return series;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching series {SeriesId} from Orthanc server", seriesId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching series {SeriesId} from Orthanc server", seriesId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching series {SeriesId} from Orthanc", seriesId);
            throw new ServerErrorException("An unexpected error occurred while fetching the series.");
        }
    }

    public async Task<OrthancInstanceListDTO> GetSeriesInstancesAsync(string seriesId, PaginationParameters? pagination = null)
    {
        try
        {
            _logger.LogInformation("Fetching instances for series {SeriesId} from Orthanc server", seriesId);

            // First, get the series to verify it exists and get the instances list
            var series = await GetSeriesByIdAsync(seriesId);
            if (series == null)
            {
                _logger.LogWarning("Series {SeriesId} not found in Orthanc server", seriesId);
                return new OrthancInstanceListDTO { Instances = new List<OrthancInstanceDTO>(), TotalCount = 0 };
            }

            if (series.Instances == null || !series.Instances.Any())
            {
                _logger.LogInformation("No instances found for series {SeriesId}", seriesId);
                return new OrthancInstanceListDTO { Instances = new List<OrthancInstanceDTO>(), TotalCount = 0 };
            }

            // Apply pagination to instance IDs
            var totalCount = series.Instances.Count;
            var paginatedInstanceIds = series.Instances;

            if (pagination != null)
            {
                var skip = (pagination.PageNumber - 1) * pagination.PageSize;
                paginatedInstanceIds = series.Instances.Skip(skip).Take(pagination.PageSize).ToList();
            }

            // Fetch detailed information for each instance
            var instancesList = new List<OrthancInstanceDTO>();

            foreach (var instanceId in paginatedInstanceIds)
            {
                try
                {
                    var instance = await GetInstanceByIdAsync(instanceId);
                    if (instance != null)
                    {
                        instancesList.Add(instance);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching instance details for ID: {InstanceId}", instanceId);
                }
            }

            _logger.LogInformation("Successfully fetched {Count} instances for series {SeriesId}", instancesList.Count, seriesId);

            return new OrthancInstanceListDTO
            {
                Instances = instancesList,
                TotalCount = totalCount
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching instances for series {SeriesId}", seriesId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching instances for series {SeriesId}", seriesId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching instances for series {SeriesId}", seriesId);
            throw new ServerErrorException("An unexpected error occurred while fetching series instances.");
        }
    }

    public async Task<OrthancInstanceDTO?> GetInstanceByIdAsync(string instanceId)
    {
        try
        {
            _logger.LogInformation("Fetching instance {InstanceId} from Orthanc server", instanceId);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/instances/{instanceId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Instance {InstanceId} not found in Orthanc server", instanceId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch instance from Orthanc server. Status: {response.StatusCode}");
            }

            var instanceJson = await response.Content.ReadAsStringAsync();
            var instance = JsonSerializer.Deserialize<OrthancInstanceDTO>(instanceJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully fetched instance {InstanceId} from Orthanc", instanceId);
            return instance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching instance {InstanceId} from Orthanc server", instanceId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching instance {InstanceId} from Orthanc server", instanceId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
                _logger.LogError(ex, "Unexpected error while fetching instance {InstanceId} from Orthanc", instanceId);
            throw new ServerErrorException("An unexpected error occurred while fetching the instance.");
        }
    }

    public async Task<byte[]> GetInstanceImageAsync(string instanceId, string format = "png")
    {
        try
        {
            _logger.LogInformation("Fetching image for instance {InstanceId} in format {Format} from Orthanc server", instanceId, format);

            // Validate format
            if (!new[] { "png", "jpg", "jpeg" }.Contains(format.ToLower()))
            {
                throw new BadRequestException($"Unsupported image format: {format}. Supported formats: png, jpg, jpeg");
            }

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/instances/{instanceId}/preview");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Instance {InstanceId} not found in Orthanc server", instanceId);
                throw new NotFoundException($"Instance {instanceId} not found in Orthanc server");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch image for instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch image from Orthanc server. Status: {response.StatusCode}");
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("Successfully fetched image for instance {InstanceId} ({Size} bytes)", instanceId, imageBytes.Length);
            
            return imageBytes;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching image for instance {InstanceId}", instanceId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching image for instance {InstanceId}", instanceId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching image for instance {InstanceId}", instanceId);
            throw new ServerErrorException("An unexpected error occurred while fetching the instance image.");
        }
    }

    public async Task<byte[]> GetInstanceDicomFileAsync(string instanceId)
    {
        try
        {
            _logger.LogInformation("Fetching DICOM file for instance {InstanceId} from Orthanc server", instanceId);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/instances/{instanceId}/file");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Instance {InstanceId} not found in Orthanc server", instanceId);
                throw new NotFoundException($"Instance {instanceId} not found in Orthanc server");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch DICOM file for instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch DICOM file from Orthanc server. Status: {response.StatusCode}");
            }

            var dicomBytes = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("Successfully fetched DICOM file for instance {InstanceId} ({Size} bytes)", instanceId, dicomBytes.Length);
            
            return dicomBytes;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching DICOM file for instance {InstanceId}", instanceId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching DICOM file for instance {InstanceId}", instanceId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching DICOM file for instance {InstanceId}", instanceId);
            throw new ServerErrorException("An unexpected error occurred while fetching the DICOM file.");
        }
    }

    public async Task<string> GetInstanceTagsAsync(string instanceId)
    {
        try
        {
            _logger.LogInformation("Fetching DICOM tags for instance {InstanceId} from Orthanc server", instanceId);

            var response = await _httpClient.GetAsync($"{_orthancBaseUrl}/instances/{instanceId}/tags?simplify");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Instance {InstanceId} not found in Orthanc server", instanceId);
                throw new NotFoundException($"Instance {instanceId} not found in Orthanc server");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch DICOM tags for instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                throw new ServerErrorException($"Failed to fetch DICOM tags from Orthanc server. Status: {response.StatusCode}");
            }

            var tagsJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully fetched DICOM tags for instance {InstanceId}", instanceId);
            
            return tagsJson;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while fetching DICOM tags for instance {InstanceId}", instanceId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching DICOM tags for instance {InstanceId}", instanceId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching DICOM tags for instance {InstanceId}", instanceId);
                throw new ServerErrorException("An unexpected error occurred while fetching the DICOM tags.");
        }
    }

    public async Task<UploadDicomResponse> UploadDicomFileAsync(IFormFile dicomFile, string userId)
    {
        try
        {
            _logger.LogInformation("Uploading DICOM file {FileName} to Orthanc server", dicomFile.FileName);

            if (dicomFile == null || dicomFile.Length == 0)
            {
                throw new BadRequestException("Invalid DICOM file provided.");
            }

            // Validate file extension
            var allowedExtensions = new[] { ".dcm", ".dicom", ".dic" };
            var fileExtension = Path.GetExtension(dicomFile.FileName).ToLower();
            
            if (!allowedExtensions.Any(ext => ext == fileExtension) && !string.IsNullOrEmpty(fileExtension))
            {
                _logger.LogWarning("File with extension {Extension} uploaded, proceeding with upload", fileExtension);
            }

            // Upload to Orthanc
            using var stream = dicomFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/dicom");

            var response = await _httpClient.PostAsync($"{_orthancBaseUrl}/instances", fileContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to upload DICOM to Orthanc. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new ServerErrorException($"Failed to upload DICOM file to Orthanc server. Status: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new ServerErrorException("Orthanc returned an empty response after upload.");
            }

            // Parse the response to get the instance ID
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            if (!responseJson.TryGetProperty("ID", out var instanceIdProperty))
            {
                throw new ServerErrorException("Invalid response from Orthanc: Missing 'ID' field.");
            }

            var instanceId = instanceIdProperty.GetString();
            if (string.IsNullOrEmpty(instanceId))
            {
                throw new ServerErrorException("Invalid instance ID received from Orthanc.");
            }

            _logger.LogInformation("DICOM file uploaded successfully. Instance ID: {InstanceId}", instanceId);

            // Get instance details to extract patient, study, and series information
            var instance = await GetInstanceByIdAsync(instanceId);
            if (instance == null)
            {
                throw new ServerErrorException("Failed to retrieve uploaded instance details.");
            }

            // Get series details
            var series = await GetSeriesByIdAsync(instance.ParentSeries);
            if (series == null)
            {
                throw new ServerErrorException("Failed to retrieve series details for uploaded instance.");
            }

            // Get study details
            var study = await GetStudyByIdAsync(series.ParentStudy);
            if (study == null)
            {
                throw new ServerErrorException("Failed to retrieve study details for uploaded instance.");
            }

            // Create or update study record in our database using the study service
            var studyStatus = await _studyService.CreateOrUpdateStudyAsync(study, userId);
            _logger.LogInformation("Study record created/updated with status: {Status}", studyStatus.ReportStatus);

            // Get patient details
            var patient = await GetPatientByIdAsync(study.ParentPatient);
            if (patient == null)
            {
                throw new ServerErrorException("Failed to retrieve patient details for uploaded instance.");
            }

            var uploadResponse = new UploadDicomResponse
            {
                OrthancInstanceId = instanceId,
                OrthancPatientId = patient.Id,
                OrthancStudyId = study.Id,
                OrthancSeriesId = series.Id,
                PatientName = patient.MainDicomTags.PatientName ?? "Unknown",
                StudyDescription = study.MainDicomTags.StudyDescription ?? "Unknown",
                Modality = series.MainDicomTags.Modality ?? "Unknown",
                StudyDate = study.MainDicomTags.StudyDate ?? "Unknown",
                Message = "DICOM file uploaded and processed successfully"
            };

            _logger.LogInformation("DICOM upload completed successfully for patient {PatientName}, study {StudyDescription}", 
                uploadResponse.PatientName, uploadResponse.StudyDescription);

            return uploadResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while uploading DICOM file to Orthanc server");
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while uploading DICOM file to Orthanc server");
            throw new ServerErrorException("Request to Orthanc server timed out during upload.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while uploading DICOM file");
                throw new ServerErrorException("An unexpected error occurred while uploading the DICOM file.");
        }
    }

    public async Task<BulkUploadDicomResponse> BulkUploadDicomFilesAsync(List<IFormFile> dicomFiles, bool generateReport = false)
    {
        var response = new BulkUploadDicomResponse
        {
            TotalFiles = dicomFiles.Count
        };

        _logger.LogInformation("Starting bulk upload of {Count} DICOM files with generateReport={GenerateReport}", dicomFiles.Count, generateReport);

        foreach (var file in dicomFiles)
        {
            try
            {
                var uploadResult = await UploadDicomFileAsync(file, userId: "system"); 
                response.SuccessfulUploads.Add(uploadResult);
                response.SuccessfulCount++;
                
                // Queue for report generation if requested
                if (generateReport)
                {
                    await QueueForReportGenerationAsync(uploadResult.OrthancInstanceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload DICOM file {FileName}", file.FileName);
                response.FailedUploads.Add(new FailedUpload
                {
                    FileName = file.FileName,
                    ErrorMessage = ex.Message
                });
                response.FailedCount++;
            }
        }

        response.Message = $"Bulk upload completed: {response.SuccessfulCount} successful, {response.FailedCount} failed out of {response.TotalFiles} files";
        
        _logger.LogInformation("Bulk upload completed: {Successful}/{Total} files uploaded successfully", 
            response.SuccessfulCount, response.TotalFiles);

        return response;
    }

    public async Task<OrthancStudyListDTO> GetAllStudiesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all studies from Orthanc server at {BaseUrl}", _orthancBaseUrl);

            // Get all study IDs
            var studyIdsResponse = await _httpClient.GetAsync($"{_orthancBaseUrl}/studies");
            if (!studyIdsResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch study IDs from Orthanc. Status: {StatusCode}", studyIdsResponse.StatusCode);
                throw new ServerErrorException($"Failed to fetch studies from Orthanc server. Status: {studyIdsResponse.StatusCode}");
            }

            var studyIds = await studyIdsResponse.Content.ReadFromJsonAsync<List<string>>();
            if (studyIds == null || !studyIds.Any())
            {
                _logger.LogInformation("No studies found in Orthanc server");
                return new OrthancStudyListDTO { Studies = new List<OrthancStudyDTO>(), TotalCount = 0 };
            }

            // Fetch detailed information for each study
            var studies = new List<OrthancStudyDTO>();
            foreach (var studyId in studyIds)
            {
                try
                {
                    var study = await GetStudyByIdAsync(studyId);
                    if (study != null)
                    {
                        studies.Add(study);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching study details for ID: {StudyId}", studyId);
                }
            }

            _logger.LogInformation("Successfully fetched {Count} studies from Orthanc", studies.Count);
            return new OrthancStudyListDTO
            {
                Studies = studies,
                TotalCount = studies.Count
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while connecting to Orthanc server");
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while connecting to Orthanc server");
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching studies from Orthanc");
            throw new ServerErrorException("An unexpected error occurred while fetching studies.");
        }
    }

    public async Task QueueForReportGenerationAsync(string instanceId)
    {
        try
        {
            _logger.LogInformation("Queueing instance {InstanceId} for report generation", instanceId);
            
            // Get the study ID from the instance
            var instance = await GetInstanceByIdAsync(instanceId);
            if (instance == null)
            {
                throw new NotFoundException($"Instance {instanceId} not found in Orthanc server");
            }

            var series = await GetSeriesByIdAsync(instance.ParentSeries);
            if (series == null)
            {
                throw new NotFoundException($"Series {instance.ParentSeries} not found in Orthanc server");
            }

            var studyId = series.ParentStudy;
            _logger.LogInformation("Found study {StudyId} for instance {InstanceId}", studyId, instanceId);

            // Get the study details and create/update it in our database if it doesn't exist
            var study = await GetStudyByIdAsync(studyId);
            if (study == null)
            {
                throw new NotFoundException($"Study {studyId} not found in Orthanc server");
            }
            
            // Update study status to queued for report generation
            var studyStatus = await _studyService.QueueStudyForReportAsync(studyId);
            _logger.LogInformation("Study {StudyId} status updated to {Status}", studyId, studyStatus.ReportStatus);
            
            // Add to the report generation queue
            GP_Server.Application.BackgroundServices.ReportQueue.Queue.Enqueue(instanceId);
            
            _logger.LogInformation("Instance {InstanceId} successfully queued for report generation", instanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing instance {InstanceId} for report generation", instanceId);
            throw;
        }
    }

    public async Task QueueStudyForReportGenerationAsync(string studyId, string userId)
    {
        try
        {
            _logger.LogInformation("Queueing study {StudyId} for report generation", studyId);
            
            // Step 1: Verify study exists in Orthanc
            var study = await GetStudyByIdAsync(studyId);
            if (study == null)
                throw new NotFoundException($"Study {studyId} not found in Orthanc server");

            // Step 2: Get study status from database
            var studyStatus = await _studyService.GetStudyStatusAsync(studyId);

            if (studyStatus == null)
            {
                // Study not in DB: create it and set status to Queued
                studyStatus = await _studyService.CreateOrUpdateStudyAsync(study, userId);
                _logger.LogInformation("Study {StudyId} created with status {Status}", studyId, studyStatus.ReportStatus);
            }
            else if (studyStatus.ReportStatus == Domain.Entities.ReportStatus.Queued)
            {
                _logger.LogInformation("Study {StudyId} is already queued, skipping.", studyId);
                return;
            }
            else if (studyStatus.ReportStatus == Domain.Entities.ReportStatus.ReportGenerated || 
                     studyStatus.ReportStatus == Domain.Entities.ReportStatus.Reviewed)
            {
                _logger.LogInformation("Study {StudyId} is completed (status: {Status}), re-queuing.", studyId, studyStatus.ReportStatus);
                studyStatus = await _studyService.QueueStudyForReportAsync(studyId);
                _logger.LogInformation("Study {StudyId} status updated to {Status}", studyId, studyStatus.ReportStatus);
            }
            else
            {
                return;
            }

            // Step 3: Queue all instances for processing
            var series = await GetStudySeriesAsync(studyId);
            var instanceIds = new List<string>();

            foreach (var seriesItem in series.Series)
            {
                var instances = await GetSeriesInstancesAsync(seriesItem.Id);
                instanceIds.AddRange(instances.Instances.Select(i => i.Id));
            }

            int enqueued = 0;

            foreach (var instanceId in instanceIds)
            {
                if (!GP_Server.Application.BackgroundServices.ReportQueue.Queue.Contains(instanceId))
                {
                    GP_Server.Application.BackgroundServices.ReportQueue.Queue.Enqueue(instanceId);
                    enqueued++;
                }
            }

            _logger.LogInformation("Study {StudyId}: {EnqueuedCount}/{TotalCount} instances queued for report generation", 
                studyId, enqueued, instanceIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing study {StudyId} for report generation", studyId);
            throw;
        }
    }

    public async Task<string> UploadDicomStructuredReportAsync(byte[] dicomSrBytes)
    {
        try
        {
            _logger.LogInformation("Uploading DICOM Structured Report to Orthanc server ({Size} bytes)", dicomSrBytes.Length);

            if (dicomSrBytes == null || dicomSrBytes.Length == 0)
            {
                throw new BadRequestException("Invalid DICOM SR data provided.");
            }

            // Upload to Orthanc
            var fileContent = new ByteArrayContent(dicomSrBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/dicom");

            var response = await _httpClient.PostAsync($"{_orthancBaseUrl}/instances", fileContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to upload DICOM SR to Orthanc. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new ServerErrorException($"Failed to upload DICOM SR to Orthanc server. Status: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new ServerErrorException("Orthanc returned an empty response after SR upload.");
            }

            // Parse the response to get the instance ID
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            if (!responseJson.TryGetProperty("ID", out var instanceIdProperty))
            {
                throw new ServerErrorException("Invalid response from Orthanc: Missing 'ID' field.");
            }

            var instanceId = instanceIdProperty.GetString();
            if (string.IsNullOrEmpty(instanceId))
            {
                throw new ServerErrorException("Invalid instance ID received from Orthanc.");
            }

            _logger.LogInformation("DICOM Structured Report uploaded successfully. Instance ID: {InstanceId}", instanceId);
            return instanceId;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while uploading DICOM SR to Orthanc server");
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while uploading DICOM SR to Orthanc server");
            throw new ServerErrorException("Request to Orthanc server timed out during SR upload.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while uploading DICOM SR");
            throw new ServerErrorException("An unexpected error occurred while uploading the DICOM Structured Report.");
        }
    }

    public async Task DeleteInstanceAsync(string instanceId)
    {
        try
        {
            _logger.LogInformation("Deleting instance {InstanceId} from Orthanc server", instanceId);

            var response = await _httpClient.DeleteAsync($"{_orthancBaseUrl}/instances/{instanceId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Instance {InstanceId} not found in Orthanc server", instanceId);
                throw new NotFoundException($"Instance {instanceId} not found in Orthanc server");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to delete instance {InstanceId} from Orthanc. Status: {StatusCode}",
                    instanceId, response.StatusCode);
                throw new ServerErrorException($"Failed to delete instance from Orthanc server. Status: {response.StatusCode}");
            }

            _logger.LogInformation("Successfully deleted instance {InstanceId} from Orthanc server", instanceId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while deleting instance {InstanceId} from Orthanc server", instanceId);
            throw new ServerErrorException("Unable to connect to Orthanc server. Please check if the server is running.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while deleting instance {InstanceId} from Orthanc server", instanceId);
            throw new ServerErrorException("Request to Orthanc server timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting instance {InstanceId} from Orthanc", instanceId);
            throw new ServerErrorException("An unexpected error occurred while deleting the instance.");
        }
    }
}
