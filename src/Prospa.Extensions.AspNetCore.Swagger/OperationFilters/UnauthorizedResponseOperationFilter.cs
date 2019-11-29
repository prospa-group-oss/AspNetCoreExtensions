﻿using System.Linq;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Prospa.Extensions.AspNetCore.Swagger.OperationFilters
{
    /// <summary>
    ///     Adds a 401 Unauthorized response to the Swagger response documentation when the authorization policy contains a
    ///     <see cref="DenyAnonymousAuthorizationRequirement" />.
    /// </summary>
    /// <seealso cref="IOperationFilter" />
    public class UnauthorizedResponseOperationFilter : IOperationFilter
    {
        private const string UnauthorizedStatusCode = "401";

        private static readonly OpenApiResponse UnauthorizedResponse = new OpenApiResponse
                                                                       {
                                                                           Description =
                                                                               "Unauthorized - The user has not supplied the necessary credentials to access the resource."
                                                                       };

        /// <summary>
        ///     Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var filterDescriptors = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var authorizationRequirements = filterDescriptors.GetPolicyRequirements();
            if (!operation.Responses.ContainsKey(UnauthorizedStatusCode) &&
                authorizationRequirements.OfType<DenyAnonymousAuthorizationRequirement>().Any())
            {
                operation.Responses.Add(UnauthorizedStatusCode, UnauthorizedResponse);
            }
        }
    }
}