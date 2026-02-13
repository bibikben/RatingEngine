using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BTSS.Rating.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRatingApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRatingService, RatingService>();
        return services;
    }
}
