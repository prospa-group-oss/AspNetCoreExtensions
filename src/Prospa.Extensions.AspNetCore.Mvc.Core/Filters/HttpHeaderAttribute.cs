﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Prospa.Extensions.AspNetCore.Mvc.Core.Filters
{
    /// <summary>
    ///     Require a HTTP header to be specified in a request and/or forwards it in the response.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    public class HttpHeaderAttribute : ActionFilterAttribute
    {
        public HttpHeaderAttribute(string httpHeaderNameName, string description)
        {
            if (httpHeaderNameName == null)
            {
                throw new ArgumentNullException(nameof(httpHeaderNameName));
            }

            if (string.IsNullOrWhiteSpace(httpHeaderNameName))
            {
                throw new ArgumentException("Http header name cannot be empty.", nameof(httpHeaderNameName));
            }

            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Http header description cannot be empty.", nameof(description));
            }

            HttpHeaderName = httpHeaderNameName;
            Description = description;
        }

        public string Description { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the specified HTTP header will be taken from the request and
        ///     forwarded to the response.
        /// </summary>
        /// <value><c>true</c> if the HTTP header is forwarded; otherwise, <c>false</c>.</value>
        public bool Forward { get; set; }

        public string HttpHeaderName { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the specified HTTP header is required. If <c>true</c> and the HTTP
        ///     header is not specified, a 400 Bad Request response will be sent.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required { get; set; }

        /// <summary>
        ///     Returns <c>true</c> if the header value is valid, otherwise <c>false</c>.
        /// </summary>
        /// <param name="headerValues">The header values.</param>
        /// <returns><c>true</c> if the specified HTTP header values are valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid(StringValues headerValues) => true;

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headerValue = context.HttpContext.Request.Headers.
                                      Where(x => string.Equals(x.Key, HttpHeaderName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).
                                      FirstOrDefault();

            var apiBehaviorOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;

            if (StringValues.IsNullOrEmpty(headerValue))
            {
                if (Required)
                {
                    var message = $"{HttpHeaderName} HTTP header is required.";
                    LogInformation(context, message);

                    context.ModelState.TryAddModelError("HTTP Header Required", message);
                    context.Result = apiBehaviorOptions.InvalidModelStateResponseFactory(context);
                }
            }
            else
            {
                if (Forward)
                {
                    context.HttpContext.Response.Headers.Add(HttpHeaderName, headerValue);
                }

                if (Required && !IsValid(headerValue))
                {
                    var message = $"{HttpHeaderName} HTTP header value '{headerValue}' is invalid.";
                    LogInformation(context, message);

                    context.ModelState.TryAddModelError("HTTP Header Required", message);
                    context.Result = apiBehaviorOptions.InvalidModelStateResponseFactory(context);
                }
            }
        }

        private static void LogInformation(ActionContext context, string message)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = factory.CreateLogger<HttpHeaderAttribute>();
            logger.LogInformation(message);
        }
    }
}
