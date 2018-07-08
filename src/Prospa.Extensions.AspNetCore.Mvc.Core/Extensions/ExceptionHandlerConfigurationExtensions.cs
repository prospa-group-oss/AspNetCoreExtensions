﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Prospa.Extensions.AspNetCore.Http;
using Prospa.Extensions.AspNetCore.Mvc.Core;
using Prospa.Extensions.AspNetCore.Mvc.Core.Resources;

// ReSharper disable CheckNamespace
namespace GlobalExceptionHandler.WebApi
// ReSharper restore CheckNamespace
{
    public static class ExceptionHandlerConfigurationExtensions
    {
        public static void HandleHttpValidationExceptions(
            this ExceptionHandlerConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            configuration.ForException<ValidationException>()
                         .ReturnStatusCode(StatusCodes.Status400BadRequest)
                         .UsingMessageFormatter((ex, context) => FormatErrorResponse(hostingEnvironment, context, ex, ex.Message));
        }

        public static void HandleOperationCancelledExceptions(
            this ExceptionHandlerConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            configuration.ForException<OperationCanceledException>()
                         .ReturnStatusCode(499)
                         .UsingMessageFormatter((ex, context) => FormatErrorResponse(hostingEnvironment, context, ex, ErrorMessages.OperationCancelled));
        }

        public static void HandleUnauthorizedExceptions(
            this ExceptionHandlerConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            configuration.ForException<UnauthorizedAccessException>()
                         .ReturnStatusCode(StatusCodes.Status401Unauthorized)
                         .UsingMessageFormatter((ex, context) => FormatErrorResponse(hostingEnvironment, context, ex, ErrorMessages.Unauthorized));
        }

        public static void HandleUnhandledExceptions(
            this ExceptionHandlerConfiguration configuration,
            IHostingEnvironment hostingEnvironment)
        {
            configuration.ContentType = "application/problem+json";
            configuration.ForException<Exception>()
                         .ReturnStatusCode(StatusCodes.Status500InternalServerError)
                         .UsingMessageFormatter((ex, context) => FormatErrorResponse(hostingEnvironment, context, ex, ErrorMessages.InternalServer));
        }

        private static string FormatErrorResponse(IHostingEnvironment hostingEnvironment, HttpContext context, Exception ex, string message)
        {
            var logger = context.RequestServices.GetRequiredService<IHttpRequestDetailsLogger>();

            if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
            {
                logger.Error(ex);
            }
            else
            {
                logger.Warning(ex);
            }

            var title = context.Response.StatusCode == StatusCodes.Status400BadRequest ? ErrorMessages.ValidationErrorTitle : message;
            var detail = context.Response.StatusCode == StatusCodes.Status500InternalServerError ? null : ex.Message;

            var errorResponse = new ValidationProblemDetails
                    {
                        Title = title,
                        Detail = detail,
                        Status = context.Response.StatusCode,
                        Instance = context.Request.Path
                    };

            if (hostingEnvironment.IsDevelopment())
            {
                errorResponse.Errors.Add("StackTrace", new[] { ex.ToString() });

                if (context.Response.StatusCode != StatusCodes.Status400BadRequest)
                {
                    errorResponse.Errors.Add("Message", new[] { ex.Message });
                }
            }

            return JsonConvert.SerializeObject(errorResponse, DefaultCamelCaseJsonSerializerSettings.Instance);
        }
    }
}