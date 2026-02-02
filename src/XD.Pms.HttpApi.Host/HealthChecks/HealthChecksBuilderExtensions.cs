using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace XD.Pms.HealthChecks;

public static class HealthChecksBuilderExtensions
{
    public static void AddPmsHealthChecks(this IServiceCollection services)
    {
        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder.AddCheck<PmsDatabaseCheck>("Pms DbContext Check", tags: ["database"]);

        services.ConfigureHealthCheckEndpoint("/health-status");
    }

    private static IServiceCollection ConfigureHealthCheckEndpoint(this IServiceCollection services, string path)
    {
        services.Configure<AbpEndpointRouterOptions>(options =>
        {
            options.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecks(
                    new PathString(path.EnsureStartsWith('/')),
                    new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                        AllowCachingResponses = false,
                    });
            });
        });

        return services;
    }
}
