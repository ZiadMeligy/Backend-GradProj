using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GP_Server.Infrastructure.BackgroundServices;

namespace GP_Server.Infrastructure.BackgroundServices
{
    public class ReportGenerationWorker : BackgroundService
    {
        private readonly ILogger<ReportGenerationWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _orthancBaseUrl = "http://localhost:8042"; // Update as needed
        private readonly string _reportEndpointUrl = "https://transformingberry-raddino-gpt2-chest-xray.hf.space/generate_report_dicom";

        public ReportGenerationWorker(IServiceProvider serviceProvider, ILogger<ReportGenerationWorker> logger, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (ReportQueue.Queue.TryDequeue(out var orthancId))
                {
                    try
                    {
                        // Download DICOM from Orthanc
                        var dicomBytes = await httpClient.GetByteArrayAsync($"{_orthancBaseUrl}/instances/{orthancId}/file");

                        // Send to report endpoint
                        using var content = new MultipartFormDataContent();
                        content.Add(new ByteArrayContent(dicomBytes), "file", "image.dcm");
                        var response = await httpClient.PostAsync(_reportEndpointUrl, content);
                        response.EnsureSuccessStatusCode();
                        var json = await response.Content.ReadAsStringAsync();

                        // TODO: Parse response, create DICOM SR, upload to Orthanc
                        _logger.LogInformation($"Report generated and uploaded for Orthanc ID: {orthancId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing report for Orthanc ID: {orthancId}");
                    }
                }
                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
