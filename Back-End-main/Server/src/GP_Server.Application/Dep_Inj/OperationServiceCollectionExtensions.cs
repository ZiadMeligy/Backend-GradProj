using GP_Server.Application.Interfaces;
using GP_Server.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GP_Server.Application.Dep_Inj;

public static class OperationServiceCollectionExtensions
{    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddScoped<AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();        services.AddScoped<IOrthancService, OrthancService>();
        services.AddScoped<IStudyService, StudyService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        return services;
    }

}
