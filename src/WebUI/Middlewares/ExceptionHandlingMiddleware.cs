using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Accounts.API.Middlewares
{
    using Accounts.Application.Common.Exceptions;
    using System;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;

            if (exception is ValidationException || exception is BadRequestException)
                code = HttpStatusCode.BadRequest;
            if (exception is ForbiddenAccessException)
                code = HttpStatusCode.Forbidden;
            if (exception is NotFoundException)
                code = HttpStatusCode.NotFound;
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(GetErrorJson(exception));
        }

        private string GetErrorJson(Exception exception)
        {
            dynamic error = new ExpandoObject();
            error.Type = exception.GetType().Name;
            error.Error = exception.Message;
            if (exception.InnerException != null)
            {
                error.Detail = GetFullMessage(exception.InnerException);
            }
            string errorSerialize = JsonSerializer.Serialize(error);
            _logger.LogError(exception,errorSerialize);
            return errorSerialize;
        }

        private string GetFullMessage(Exception ex)
        {
            return ex.InnerException == null
                 ? ex.Message
                 : ex.Message + " " + GetFullMessage(ex.InnerException);
        }
    }
}