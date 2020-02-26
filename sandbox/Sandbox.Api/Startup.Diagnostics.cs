﻿using GlobalExceptionHandler.WebApi;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Prospa.Extensions.ApplicationInsights;
using Prospa.Extensions.AspNetCore.ApplicationInsights;
using Prospa.Extensions.AspNetCore.Http;
using Prospa.Extensions.AspNetCore.Http.Builder;
using Prospa.Extensions.AspNetCore.Serilog;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
    // ReSharper restore CheckNamespace
{
    public static class StartupDiagnostics
    {
        public static IServiceCollection AddDefaultDiagnostics(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HttpErrorLogOptions>(configuration.GetSection(nameof(HttpErrorLogOptions)));
            services.AddSingleton(provider => provider.GetRequiredService<IOptions<HttpErrorLogOptions>>().Value);
            services.AddScoped<IHttpRequestDetailsLogger, HttpRequestDetailsSerilogLogger>();

            services.AddSingleton<ITelemetryInitializer, ActivityTagTelemetryInitializer>();
            services.AddApplicationInsightsTelemetryProcessor<AzureDependencyTelemetryProcessor>();

            return services;
        }

        public static IApplicationBuilder UseDefaultDiagnostics(this IApplicationBuilder app, IWebHostEnvironment hostingEnvironment)
        {
            app.UseMiddleware<LogEnrichmentMiddleware>();

            app.UseDiagnosticActivityTagging();

            app.UseGlobalExceptionHandler(
                configuration =>
                {
                    configuration.HandleHttpValidationExceptions(hostingEnvironment);
                    configuration.HandleOperationCancelledExceptions(hostingEnvironment);
                    configuration.HandleUnauthorizedExceptions(hostingEnvironment);
                    configuration.HandleUnhandledExceptions(hostingEnvironment);
                });

            return app;
        }
    }
}
