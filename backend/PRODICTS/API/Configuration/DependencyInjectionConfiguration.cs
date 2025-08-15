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

        // Service Dependencies
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
    }
}
