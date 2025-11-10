using AutoMapper;
using GP_Server.Application.DTOs;
using GP_Server.Application.DTOs.Reports;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Application.Exceptions;
using GP_Server.Application.Helpers;
using GP_Server.Application.Interfaces;
using GP_Server.Domain.Entities;
using GP_Server.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GP_Server.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IRepository<Study> _studyRepository;
    private readonly IOrthancService _orthancService;
    private readonly IStudyService _studyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IRepository<Study> studyRepository,
        IOrthancService orthancService,
        IStudyService studyService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DoctorService> logger)
    {
        _studyRepository = studyRepository;
        _orthancService = orthancService;
        _studyService = studyService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StudyStatusDTO>> GetStudiesAssignedToDoctorAsync(string doctorId)
    {
        try
        {
            _logger.LogInformation("Fetching studies assigned to doctor: {DoctorId}", doctorId);

            var studies = await _studyRepository.FindAsync(s => s.AssignedDoctorId == doctorId);

            var studyList = studies.OrderByDescending(s => s.CreatedAt).ToList();

            _logger.LogInformation("Found {Count} studies assigned to doctor: {DoctorId}", studyList.Count, doctorId);

            return _mapper.Map<List<StudyStatusDTO>>(studyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching studies assigned to doctor: {DoctorId}", doctorId);
            throw new ServerErrorException("An error occurred while retrieving assigned studies.");
        }
    }

    public async Task<(List<object> Studies, int TotalCount)> GetStudiesAssignedToDoctorWithFullDataAsync(string doctorId, PaginationParameters? pagination = null)
    {
        try
        {
            _logger.LogInformation("Fetching studies with full data assigned to doctor: {DoctorId}", doctorId);

            // Get all studies assigned to the doctor
            var studies = await _studyRepository.FindAsync(s => s.AssignedDoctorId == doctorId);
            var studyList = studies.OrderByDescending(s => s.CreatedAt).ToList();
            var totalCount = studyList.Count;

            _logger.LogInformation("Found {Count} studies assigned to doctor: {DoctorId}", totalCount, doctorId);

            // Apply pagination if provided
            if (pagination != null)
            {
                var skip = (pagination.PageNumber - 1) * pagination.PageSize;
                studyList = studyList.Skip(skip).Take(pagination.PageSize).ToList();
            }

            // For each study, get the patient info and study details from Orthanc
            var result = new List<object>();
            foreach (var study in studyList)
            {
                try
                {
                    // Get study from Orthanc
                    var orthancStudy = await _orthancService.GetStudyByIdAsync(study.OrthancStudyId);

                    // Get patient info from Orthanc if study exists
                    object? patient = null;
                    if (orthancStudy != null && !string.IsNullOrEmpty(orthancStudy.ParentPatient))
                    {
                        patient = await _orthancService.GetPatientByIdAsync(orthancStudy.ParentPatient);
                    }

                    // Get study status
                    var status = _mapper.Map<StudyStatusDTO>(study);

                    result.Add(new
                    {
                        Study = orthancStudy,
                        Patient = patient,
                        Status = status
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get Orthanc data for study {StudyId}, including status only", study.OrthancStudyId);

                    // If Orthanc data fails, still include the status
                    var status = _mapper.Map<StudyStatusDTO>(study);
                    result.Add(new
                    {
                        Study = (object?)null,
                        Patient = (object?)null,
                        Status = status
                    });
                }
            }

            return (result, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching studies with full data assigned to doctor: {DoctorId}", doctorId);
            throw new ServerErrorException("An error occurred while retrieving assigned studies with full data.");
        }
    }

    public async Task<StudyStatusDTO> ReviewReportAsync(string studyId, string doctorId, ReviewReportDTO reviewReportDto)
    {
        try
        {
            _logger.LogInformation("Starting review process for study: {StudyId} by doctor: {DoctorId}", studyId, doctorId);

            // Get study by id (using studyId as OrthancStudyId)
            var studies = await _studyRepository.FindAsync(s => s.OrthancStudyId == studyId);
            var study = studies.FirstOrDefault();

            if (study == null)
            {
                throw new NotFoundException($"Study with ID {studyId} not found.");
            }

            // Verify the doctor is assigned to this study
            if (study.AssignedDoctorId != doctorId)
            {
                throw new UnAuthorizedException("You are not assigned to review this study.");
            }

            // Get study details from Orthanc to retrieve patient information
            var orthancStudy = await _orthancService.GetStudyByIdAsync(studyId);
            if (orthancStudy == null)
            {
                throw new NotFoundException($"Study {studyId} not found in Orthanc server.");
            }

            // Get patient details from Orthanc
            var patient = await _orthancService.GetPatientByIdAsync(orthancStudy.ParentPatient);
            if (patient == null)
            {
                throw new NotFoundException($"Patient data not found for study {studyId}.");
            }

            string reviewInstanceId;

            // Check if there's already a generated SR report to replace
            if (!string.IsNullOrEmpty(study.GeneratedReportInstanceId))
            {
                _logger.LogInformation("Replacing existing SR report: {ReportInstanceId}", study.GeneratedReportInstanceId);
                
                // Delete the existing SR report from Orthanc
                try
                {
                    await _orthancService.DeleteInstanceAsync(study.GeneratedReportInstanceId);
                    _logger.LogInformation("Successfully deleted existing SR report: {ReportInstanceId}", study.GeneratedReportInstanceId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete existing SR report: {ReportInstanceId}. Proceeding with new report creation.", study.GeneratedReportInstanceId);
                }
            }

            _logger.LogInformation("Creating new SR report for study: {StudyId}", studyId);
            
            // Create new DICOM SR with the reviewed findings and impressions
            var newSrBytes = await DicomStructuredReportHelper.GenerateStructuredSR(
                reviewReportDto.Findings,
                reviewReportDto.Impressions,
                patient.MainDicomTags.PatientID ?? "",
                patient.MainDicomTags.PatientName ?? "",
                orthancStudy.MainDicomTags.StudyInstanceUID ?? "",
                study.GeneratedReportInstanceId ?? "" // Reference to original if it existed
            );

            // Upload the new SR to Orthanc
            reviewInstanceId = await _orthancService.UploadDicomStructuredReportAsync(newSrBytes);
            
            _logger.LogInformation("Successfully created new SR report. Instance ID: {NewInstanceId}", reviewInstanceId);

            // Update the study with the new report instance ID and set status to Reviewed
            study.GeneratedReportInstanceId = reviewInstanceId;
            study.ReportGeneratedAt = DateTime.UtcNow;
            study.ReportStatus = Domain.Entities.ReportStatus.Reviewed;
            
            await _studyRepository.UpdateAsync(study);
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Study {StudyId} updated with reviewed report instance ID: {ReviewInstanceId} and status set to Reviewed", studyId, reviewInstanceId);

            // Return the updated study information
            var updatedStudyDto = _mapper.Map<StudyStatusDTO>(study);
            
            _logger.LogInformation("Review completed successfully for study: {StudyId}", studyId);
            
            return updatedStudyDto;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (UnAuthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing report for study: {StudyId} by doctor: {DoctorId}", studyId, doctorId);
            throw new ServerErrorException("An error occurred while reviewing the report.");
        }
    }
    
}
