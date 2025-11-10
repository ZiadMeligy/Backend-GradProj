using AutoMapper;
using GP_Server.Application.DTOs.Studies;
using GP_Server.Domain.Entities;

namespace GP_Server.Application.MappingProfiles.Studies;

public class StudyProfile : Profile
{
    public StudyProfile()
    {
        CreateMap<Study, StudyStatusDTO>();
        CreateMap<CreateStudyDTO, Study>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReportStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ReportQueuedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReportGeneratedAt, opt => opt.Ignore())
            .ForMember(dest => dest.GeneratedReportInstanceId, opt => opt.Ignore())
            .ForMember(dest => dest.ReportGenerationError, opt => opt.Ignore())
            .ForMember(dest => dest.ReportGenerationAttempts, opt => opt.Ignore());
    }
}
