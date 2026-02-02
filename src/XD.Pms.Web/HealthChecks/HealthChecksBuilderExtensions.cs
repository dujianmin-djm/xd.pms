using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace XD.Pms.Web.HealthChecks;

public static class HealthChecksBuilderExtensions
{
    public static void AddPmsHealthChecksUi(this IServiceCollection services)
    {
        var configuration = services.GetConfiguration();
        var healthChecksUiBuilder = services.AddHealthChecksUI(settings =>
        {
			settings.SetEvaluationTimeInSeconds(600);       // 每60秒检查一次
			settings.MaximumHistoryEntriesPerEndpoint(50);  // 最大历史记录
			settings.SetApiMaxActiveRequests(1);            // 并发请求数

			settings.AddHealthCheckEndpoint("Pms Health Status", configuration["App:HealthUiCheckUrl"]!);
        });

		/* Set HealthCheck UI Storage Install-Package 
		 * AspNetCore.HealthChecks.UI
		 * AspNetCore.HealthChecks.UI.Client 用于UIResponseWriter.WriteHealthCheckUIResponse
		 * AspNetCore.HealthChecks.UI.InMemory.Storage
		 * Microsoft.EntityFrameworkCore.InMemory
         */
		healthChecksUiBuilder.AddInMemoryStorage();

        services.MapHealthChecksUiEndpoints(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-api";
        });
    }

    private static IServiceCollection MapHealthChecksUiEndpoints(this IServiceCollection services, Action<global::HealthChecks.UI.Configuration.Options>? setupOption = null)
    {
        services.Configure<AbpEndpointRouterOptions>(routerOptions =>
        {
            routerOptions.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecksUI(setupOption);
            });
        });

        return services;
    }
}
