using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace XD.Pms.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class PmsDbContextFactory : IDesignTimeDbContextFactory<PmsDbContext>
{
    public PmsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        PmsEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<PmsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new PmsDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../XD.Pms.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
