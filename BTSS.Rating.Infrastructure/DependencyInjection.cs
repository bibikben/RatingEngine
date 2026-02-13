using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BTSS.Rating.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRatingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Connection string key: ConnectionStrings:RatingDb
        var cs = configuration.GetConnectionString("RatingDb") ?? "Server=.;Database=RatingDb;Trusted_Connection=True;TrustServerCertificate=True";
        services.AddDbContext<RatingDbContext>(opt => opt.UseSqlServer(cs));

        services.AddScoped<BTSS.Rating.Application.Abstractions.IRatingService, BTSS.Rating.Infrastructure.Services.DbRatingService>();
        services.AddScoped<BTSS.Rating.Application.Abstractions.IContractLookup, BTSS.Rating.Infrastructure.Services.ContractLookup>();
        // TODO: register repositories / query services here
        services.AddScoped<BTSS.Rating.Application.Abstractions.IRatingCommitService, BTSS.Rating.Infrastructure.Services.RatingCommitService>();
    services.AddScoped<BTSS.Rating.Application.Abstractions.IContractVersionPublisher, BTSS.Rating.Infrastructure.Services.ContractVersionPublisher>();
        // TODO: register repositories / query services here
        return services;
    }
}
