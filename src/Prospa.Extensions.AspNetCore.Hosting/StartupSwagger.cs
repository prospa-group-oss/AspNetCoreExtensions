﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Prospa.Extensions.AspNetCore.Authorization;
using Prospa.Extensions.AspNetCore.Mvc.Versioning.Swagger.DocumentFilters;
using Prospa.Extensions.AspNetCore.Mvc.Versioning.Swagger.OperationFilters;
using Prospa.Extensions.AspNetCore.Swagger;
using Prospa.Extensions.AspNetCore.Swagger.OperationFilters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Builder
    // ReSharper restore CheckNamespace
{
    public static class StartupSwagger
    {
        public static IServiceCollection AddProspaDefaultSwagger(this IServiceCollection services, Assembly startupAssembly)
        {
            var provider = services.BuildServiceProvider();

            services.AddSwaggerGen(
                options =>
                {
                    var assemblyDescription = startupAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
                    var apiVersionDescriptionProvider = provider.GetRequiredService<IApiVersionDescriptionProvider>();

                    options.SwaggerVersionedDoc(apiVersionDescriptionProvider, assemblyDescription, startupAssembly.GetName().Name);
                    options.AllowFilteringDocsByApiVersion();

                    AddDefaultOptions(options, startupAssembly);
                    AddDefaultOperationFilters(provider, options);
                    AddDefaultDocumentFilters(options);
                });

            return services;
        }

        public static IApplicationBuilder UseProspaDefaultSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(
                options =>
                {
                    options.PreSerializeFilters.Add((swagger, httpReq) =>
                    {
                        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
                    });
                });

            app.UseSwaggerUI(
                options =>
                {
                    var provider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                    options.SwaggerVersionedJsonEndpoints(provider);
                });

            return app;
        }

        private static void AddDefaultDocumentFilters(SwaggerGenOptions options)
        {
            options.DocumentFilter<SetVersionInPaths>();
        }

        private static void AddDefaultOperationFilters(IServiceProvider provider, SwaggerGenOptions options)
        {
            var authzOptions = provider.GetRequiredService<AuthOptions>();

            options.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>(authzOptions.ScopePolicies);
            options.OperationFilter<RemoveVersionParameters>();
            options.OperationFilter<HttpHeaderOperationFilter>();
            options.OperationFilter<ForbiddenResponseOperationFilter>();
            options.OperationFilter<UnauthorizedResponseOperationFilter>();
            options.OperationFilter<DelimitedQueryStringOperationFilter>();
            options.OperationFilter<DeprecatedVersionOperationFilter>();
        }

        private static void AddDefaultOptions(SwaggerGenOptions options, Assembly assembly)
        {
            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();
            options.DescribeAllParametersInCamelCase();
            options.IncludeXmlCommentsIfExists(assembly);
        }
    }
}