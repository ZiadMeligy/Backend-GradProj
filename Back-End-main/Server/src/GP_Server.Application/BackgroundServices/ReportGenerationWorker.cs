using GP_Server.Application.Interfaces;
using GP_Server.Application.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace GP_Server.Application.BackgroundServices
{
    public class ReportGenerationWorker : BackgroundService
    {
        private readonly ILogger<ReportGenerationWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _orthancBaseUrl;
        private readonly string _reportEndpointUrl;
        private readonly string? _orthancUsername;
        private readonly string? _orthancPassword;

        public ReportGenerationWorker(IServiceProvider serviceProvider, ILogger<ReportGenerationWorker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _orthancBaseUrl = configuration["Orthanc:BaseUrl"] ?? configuration["Orthanc__BaseUrl"] ?? "http://orthanc:8042";
            _reportEndpointUrl = configuration["AI:Endpoint"] ?? "http://localhost:8000/generate_report_dicom";
            _orthancUsername = configuration["Orthanc:Username"] ?? configuration["Orthanc__Username"];
            _orthancPassword = configuration["Orthanc:Password"] ?? configuration["Orthanc__Password"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            // Set Basic Auth header for Orthanc if credentials are present
            if (!string.IsNullOrEmpty(_orthancUsername) && !string.IsNullOrEmpty(_orthancPassword))
            {
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_orthancUsername}:{_orthancPassword}");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (ReportQueue.Queue.TryDequeue(out var orthancId))
                {
                    try
                    {
                        // Download DICOM from Orthanc
                        var dicomBytes = await httpClient.GetByteArrayAsync($"{_orthancBaseUrl}/instances/{orthancId}/file");

                        // Get original DICOM instance information to associate SR with same study
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var orthancService = scope.ServiceProvider.GetRequiredService<IOrthancService>();
                            var studyService = scope.ServiceProvider.GetRequiredService<IStudyService>();
                            
                            // Get instance details
                            var instance = await orthancService.GetInstanceByIdAsync(orthancId);
                            if (instance == null)
                            {
                                _logger.LogError($"Could not retrieve instance details for Orthanc ID: {orthancId}");
                                continue;
                            }

                            // Get series details
                            var series = await orthancService.GetSeriesByIdAsync(instance.ParentSeries);
                            if (series == null)
                            {
                                _logger.LogError($"Could not retrieve series details for instance: {orthancId}");
                                continue;
                            }

                            // Get study details
                            var study = await orthancService.GetStudyByIdAsync(series.ParentStudy);
                            if (study == null)
                            {
                                _logger.LogError($"Could not retrieve study details for instance: {orthancId}");
                                continue;
                            }

                            // Update study status to indicate report generation has started
                            try
                            {
                                await studyService.MarkReportAsInProgressAsync(study.Id);
                                _logger.LogInformation($"Study record created/updated and marked as InProgress for Orthanc ID: {study.Id}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to create/update study record or mark as InProgress for Orthanc ID: {study.Id}");
                            }

                            // Get patient details
                            var patient = await orthancService.GetPatientByIdAsync(study.ParentPatient);
                            if (patient == null)
                            {
                                _logger.LogError($"Could not retrieve patient details for instance: {orthancId}");
                                continue;
                            }

                            // Send to report endpoint
                            _logger.LogInformation($"Generating report for Orthanc ID: {orthancId}");
                            using var content = new MultipartFormDataContent();
                            content.Add(new ByteArrayContent(dicomBytes), "file", "image.dcm");
                            var response = await httpClient.PostAsync(_reportEndpointUrl, content);
                            response.EnsureSuccessStatusCode();
                            var json = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"Received response from AI endpoint for Orthanc ID {orthancId}: {json}");

                            // Parse the report text from the AI endpoint response
                            var reportResponse = JsonSerializer.Deserialize<ReportResponse>(json);
                            var reportText = FormatReportText(reportResponse?.generated_report);

                            // Log the generated report
                            _logger.LogInformation($"Generated report for Orthanc ID {orthancId}: {reportText}");

                            // Generate DICOM Structured Report (SR)
                            try
                            {
                                _logger.LogInformation($"Creating DICOM Structured Report for instance {orthancId}");
                                
                                // Create DICOM SR with the generated report
                                var dicomSrBytes = await DicomStructuredReportHelper.GenerateBasicTextSR(
                                    reportText,
                                    patient.MainDicomTags.PatientID ?? "",
                                    patient.MainDicomTags.PatientName ?? "",
                                    study.MainDicomTags.StudyInstanceUID ?? "",
                                    instance.MainDicomTags.SOPInstanceUID ?? "" // Use actual DICOM SOP Instance UID
                                );

                                // Upload the SR back to Orthanc
                                var srInstanceId = await orthancService.UploadDicomStructuredReportAsync(dicomSrBytes);
                                
                                // Update study status to indicate successful report generation
                                try
                                {
                                    await studyService.MarkReportAsGeneratedAsync(study.Id, srInstanceId);
                                    _logger.LogInformation($"Study status updated to ReportGenerated for Orthanc ID: {study.Id}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Failed to update study status for Orthanc ID: {study.Id}");
                                }
                                
                                _logger.LogInformation($"Successfully created and uploaded DICOM SR. New instance ID: {srInstanceId}");
                                _logger.LogInformation($"Report generation completed for Orthanc ID: {orthancId}. SR Instance: {srInstanceId}");
                            }
                            catch (Exception srEx)
                            {
                                _logger.LogError(srEx, $"Failed to create or upload DICOM SR for instance {orthancId}. Report text: {reportText}");
                                
                                // Update study status to indicate failed report generation
                                try
                                {
                                    await studyService.MarkReportAsFailedAsync(study.Id, srEx.Message);
                                    _logger.LogInformation($"Study status updated to Failed for Orthanc ID: {study.Id}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Failed to update study status to Failed for Orthanc ID: {study.Id}");
                                }
                                
                                // Still log the report even if SR creation fails
                                _logger.LogInformation($"Report generation completed (SR failed) for Orthanc ID: {orthancId}. Report: {reportText}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing report for Orthanc ID: {orthancId}");
                        
                        // Try to mark study as failed if we can get the study ID
                        try
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var orthancService = scope.ServiceProvider.GetRequiredService<IOrthancService>();
                                var studyService = scope.ServiceProvider.GetRequiredService<IStudyService>();
                                
                                var instance = await orthancService.GetInstanceByIdAsync(orthancId);
                                if (instance != null)
                                {
                                    var series = await orthancService.GetSeriesByIdAsync(instance.ParentSeries);
                                    if (series != null)
                                    {
                                        var study = await orthancService.GetStudyByIdAsync(series.ParentStudy);
                                        if (study != null)
                                        {
                                            await studyService.MarkReportAsFailedAsync(study.Id, ex.Message);
                                            _logger.LogInformation($"Study status updated to Failed for Orthanc ID: {study.Id} due to processing error");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogError(updateEx, $"Failed to update study status to Failed for instance {orthancId}");
                        }
                    }
                }
                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private string FormatReportText(ReportData? reportData)
        {
            if (reportData == null)
                return "No report generated.";

            var report = "";
            
            if (!string.IsNullOrEmpty(reportData.findings))
            {
                report += "Findings:\n" + reportData.findings + "\n\n";
            }
            
            if (!string.IsNullOrEmpty(reportData.impression))
            {
                report += "Impressions:\n" + reportData.impression;
            }
            
            return string.IsNullOrEmpty(report) ? "No report generated." : report;
        }

        private class ReportResponse
        {
            public ReportData? generated_report { get; set; }
            public DicomMetadata? dicom_metadata { get; set; }
            public ImageInfo? image_info { get; set; }
        }

        private class ReportData
        {
            public string? findings { get; set; }
            public string? impression { get; set; }
        }

        private class DicomMetadata
        {
            public string? patient_id { get; set; }
            public string? study_date { get; set; }
            public string? modality { get; set; }
            public string? body_part { get; set; }
        }

        private class ImageInfo
        {
            public int width { get; set; }
            public int height { get; set; }
            public string? mode { get; set; }
        }
    }
}
