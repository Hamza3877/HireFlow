using HireFlow.Application.Interfaces;
using HireFlow.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HireFlow.Application;

// One method that registers every service of the Application layer.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddSingleton<IMatchScoreCalculator, MatchScoreCalculator>();
        return services;
    }
}
