using Application.Interface;
using Application.Services;
using Domain.Interfaces;
using Persistence.Context;
using Persistence.Repositories;
using Microsoft.Extensions.Options;

namespace API.Configuration;

public static class DependencyInjectionConfiguration
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB Configuration
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDbSettings"));

        services.AddSingleton<MongoDbContext>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>();
            return new MongoDbContext(settings);
        });

        // Repository Dependencies
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAnonymousUserRepository, AnonymousUserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IFlashCardRepository, FlashCardRepository>();
        services.AddScoped<IFlashCardGroupRepository, FlashCardGroupRepository>();
        services.AddScoped<IAppConfigRepository, AppConfigRepository>();
        services.AddScoped<IPodcastSeriesRepository, PodcastSeriesRepository>();
        services.AddScoped<IPodcastSeasonRepository, PodcastSeasonRepository>();
        services.AddScoped<IPodcastEpisodeRepository, PodcastEpisodeRepository>();
        services.AddScoped<IPodcastQuizRepository, PodcastQuizRepository>();

        // Service Dependencies
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFlashCardService, FlashCardService>();
        services.AddScoped<IFlashCardGroupService, FlashCardGroupService>();
        services.AddScoped<IAppConfigService, AppConfigService>();
        services.AddScoped<IPodcastSeriesService, PodcastSeriesService>();
        services.AddScoped<IPodcastSeasonService, PodcastSeasonService>();
        services.AddScoped<IPodcastEpisodeService, PodcastEpisodeService>();
        services.AddScoped<IPodcastQuizService, PodcastQuizService>(); 
    }
}
