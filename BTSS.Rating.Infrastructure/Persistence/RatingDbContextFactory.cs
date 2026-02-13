using BTSS.Rating.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BTSS.Rating.Infrastructure.Persistence;

public sealed class RatingDbContextFactory : IDesignTimeDbContextFactory<RatingDbContext>
{
    public RatingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RatingDbContext>()
            .UseSqlServer("Server=.;Database=RatingDb;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new RatingDbContext(options);
    }
}