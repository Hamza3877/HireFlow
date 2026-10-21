using HireFlow.Application.Interfaces;
using HireFlow.Infrastructure.Data;
using HireFlow.Infrastructure.Files;
using HireFlow.Infrastructure.Notifications;
using HireFlow.Infrastructure.Repositories;
using HireFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HireFlow.Infrastructure;

// One method that registers everything the Infrastructure layer provides.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database: SQL Server by default, SQLite for the free live demo.
        var provider = config["Database:Provider"] ?? "SqlServer";
        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider == "Sqlite")
            {
                options.UseSqlite(config.GetConnectionString("Sqlite") ?? "Data Source=hireflow.db");
            }
            else
            {
                options.UseSqlServer(config.GetConnectionString("SqlServer"));
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IInterviewRepository, InterviewRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        services.AddSingleton<ICvStorage, LocalCvStorage>();
        services.AddSingleton<ICvTextExtractor, CvTextExtractor>();

        // Notifications: a separate service over HTTP, or in the same process for demo hosting.
        var mode = config["Notifications:Mode"] ?? "InProcess";
        if (mode == "Http")
        {
            var baseUrl = config["Notifications:BaseUrl"] ?? "http://localhost:5001/";
            services.AddHttpClient<INotificationClient, HttpNotificationClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            });
        }
        else
        {
            services.AddScoped<INotificationClient, InProcessNotificationClient>();
        }

        return services;
    }
}
