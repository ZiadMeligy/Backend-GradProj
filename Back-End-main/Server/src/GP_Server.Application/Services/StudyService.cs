using AutoMapper;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Application.Exceptions;
using GP_Server.Application.Interfaces;
using GP_Server.Domain.Entities;
using GP_Server.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GP_Server.Application.Services;

public class StudyService : IStudyService
{
    private readonly IRepository<Study> _studyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StudyService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudyService(
        IRepository<Study> studyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ILogger<StudyService> logger)
    {
        _studyRepository = studyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<StudyStatusDTO> CreateOrUpdateStudyAsync(OrthancStudyDTO orthancStudy, string userId)
    {
        try
        {
            // Check if study already exists
            var existingStudies = await _studyRepository.FindAsync(s => s.OrthancStudyId == orthancStudy.Id);
            var study = existingStudies.FirstOrDefault();

            if (study == null)
            {
                // Create new study
                study = new Study
                {
                    OrthancStudyId = orthancStudy.Id,
                    StudyInstanceUID = orthancStudy.MainDicomTags.StudyInstanceUID,
                    StudyDescription = orthancStudy.MainDicomTags.StudyDescription,
                    StudyDate = orthancStudy.MainDicomTags.StudyDate,
                    ReportStatus = ReportStatus.NoReport
                };

                await _studyRepository.AddAsync(study);
                _logger.LogInformation("Created new study record for Orthanc ID: {OrthancStudyId}", orthancStudy.Id);
            }
            else
            {
                // Update existing study metadata if needed
                study.StudyInstanceUID = orthancStudy.MainDicomTags.StudyInstanceUID;
                study.StudyDescription = orthancStudy.MainDicomTags.StudyDescription;
                study.StudyDate = orthancStudy.MainDicomTags.StudyDate;

                await _studyRepository.UpdateAsync(study);
                _logger.LogInformation("Updated study record for Orthanc ID: {OrthancStudyId}", orthancStudy.Id);
            }

            await _unitOfWork.CommitAsync();
            return _mapper.Map<StudyStatusDTO>(study);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or updating study for Orthanc ID: {OrthancStudyId}", orthancStudy.Id);
            throw new ServerErrorException("An error occurred while creating or updating the study.");
        }
    }

    public async Task<StudyStatusDTO?> GetStudyStatusAsync(string orthancStudyId)
    {
        try
        {
            var studies = await _studyRepository.FindAsync(s => s.OrthancStudyId == orthancStudyId);
            var study = studies.FirstOrDefault();

            return study == null ? null : _mapper.Map<StudyStatusDTO>(study);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting study status for Orthanc ID: {OrthancStudyId}", orthancStudyId);
            throw new ServerErrorException("An error occurred while retrieving the study status.");
        }
    }

    public async Task<StudyWithStatusListDTO> GetAllStudiesWithStatusAsync(int pageNumber = 1, int pageSize = 50, ReportStatus? statusFilter = null)
    {
        try
        {
            var studies = await _studyRepository.GetAllAsync();

            // Apply status filter if provided
            if (statusFilter.HasValue)
            {
                studies = studies.Where(s => s.ReportStatus == statusFilter.Value);
            }

            var totalCount = studies.Count();

            // Apply pagination
            var paginatedStudies = studies
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new StudyWithStatusListDTO
            {
                Studies = _mapper.Map<List<StudyStatusDTO>>(paginatedStudies),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting studies with status");
            throw new ServerErrorException("An error occurred while retrieving studies with status.");
        }
    }

    public async Task<StudyStatusDTO> QueueStudyForReportAsync(string orthancStudyId)
    {
        return await UpdateStudyStatusAsync(orthancStudyId, ReportStatus.Queued);
    }

    public async Task<StudyStatusDTO> MarkReportAsInProgressAsync(string orthancStudyId)
    {
        return await UpdateStudyStatusAsync(orthancStudyId, ReportStatus.InProgress);
    }

    public async Task<StudyStatusDTO> MarkReportAsGeneratedAsync(string orthancStudyId, string reportInstanceId)
    {
        try
        {
            var studies = await _studyRepository.FindAsync(s => s.OrthancStudyId == orthancStudyId);
            var study = studies.FirstOrDefault();

            if (study == null)
            {
                throw new NotFoundException($"Study with Orthanc ID {orthancStudyId} not found.");
            }

            study.ReportStatus = ReportStatus.ReportGenerated;
            study.ReportGeneratedAt = DateTime.UtcNow;
            study.GeneratedReportInstanceId = reportInstanceId;
            study.ReportGenerationError = null;

            await _studyRepository.UpdateAsync(study);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Marked report as generated for study: {OrthancStudyId}", orthancStudyId);
            return _mapper.Map<StudyStatusDTO>(study);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking report as generated for study: {OrthancStudyId}", orthancStudyId);
            throw new ServerErrorException("An error occurred while updating the study status.");
        }
    }

    public async Task<StudyStatusDTO> MarkReportAsFailedAsync(string orthancStudyId, string errorMessage)
    {
        try
        {
            var studies = await _studyRepository.FindAsync(s => s.OrthancStudyId == orthancStudyId);
            var study = studies.FirstOrDefault();

            if (study == null)
            {
                throw new NotFoundException($"Study with Orthanc ID {orthancStudyId} not found.");
            }

            study.ReportStatus = ReportStatus.Failed;
            study.ReportGenerationError = errorMessage;
            study.ReportGenerationAttempts++;

            await _studyRepository.UpdateAsync(study);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Marked report as failed for study: {OrthancStudyId}", orthancStudyId);
            return _mapper.Map<StudyStatusDTO>(study);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking report as failed for study: {OrthancStudyId}", orthancStudyId);
            throw new ServerErrorException("An error occurred while updating the study status.");
        }
    }

    public async Task<List<StudyStatusDTO>> GetStudiesByStatusAsync(ReportStatus status)
    {
        try
        {
            var studies = await _studyRepository.FindAsync(s => s.ReportStatus == status);
            return _mapper.Map<List<StudyStatusDTO>>(studies.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting studies by status: {Status}", status);
            throw new ServerErrorException("An error occurred while retrieving studies by status.");
        }
    }

    private async Task<StudyStatusDTO> UpdateStudyStatusAsync(string orthancStudyId, ReportStatus status)
    {
        try
        {
            var studies = await _studyRepository.FindAsync(s => s.OrthancStudyId == orthancStudyId);
            var study = studies.FirstOrDefault();

            if (study == null)
            {
                throw new NotFoundException($"Study with Orthanc ID {orthancStudyId} not found.");
            }

            study.ReportStatus = status;

            if (status == ReportStatus.Queued)
            {
                study.ReportQueuedAt = DateTime.UtcNow;
                study.ReportGenerationError = null;
            }
            else if (status == ReportStatus.InProgress)
            {
                study.ReportGenerationError = null;
            }

            await _studyRepository.UpdateAsync(study);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Updated study status to {Status} for Orthanc ID: {OrthancStudyId}", status, orthancStudyId);
            return _mapper.Map<StudyStatusDTO>(study);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating study status for Orthanc ID: {OrthancStudyId}", orthancStudyId);
            throw new ServerErrorException("An error occurred while updating the study status.");
        }
    }

    public async Task AssignStudyToDoctorAsync(string orthancStudyId, string doctorId)
    {
        try
        {
            var studies = await _studyRepository.FindAsync(s => s.OrthancStudyId == orthancStudyId);
            var study = studies.FirstOrDefault();

            if (study == null)
            {
                throw new NotFoundException($"Study with Orthanc ID {orthancStudyId} not found.");
            }

            var user = await _userManager.FindByIdAsync(doctorId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                throw new NotFoundException($"Doctor with ID {doctorId} not found or is not a doctor.");
            }

            // Assuming Study has a DoctorId property to assign the doctor
            study.AssignedDoctorId = doctorId;

            await _studyRepository.UpdateAsync(study);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Assigned study {OrthancStudyId} to doctor {DoctorId}", orthancStudyId, doctorId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning study {OrthancStudyId} to doctor {DoctorId}", orthancStudyId, doctorId);
            throw new ServerErrorException("An error occurred while assigning the study to the doctor.");
        }
    }

}


