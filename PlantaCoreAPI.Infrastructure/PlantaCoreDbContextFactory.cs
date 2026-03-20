using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PlantaCoreAPI.Infrastructure.Dados;
using System.IO;

namespace PlantaCoreAPI.Infrastructure;

public class PlantaCoreDbContextFactory : IDesignTimeDbContextFactory<PlantaCoreDbContext>
{
    public PlantaCoreDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "PlantaCoreAPI.API");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PlantaCoreDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        return new PlantaCoreDbContext(optionsBuilder.Options);
    }
}