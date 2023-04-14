using DCode_GlobalErrorHandlingMiddleware.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Net;
using System.Text.Json;

namespace DCode_GlobalErrorHandlingMiddleware
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            string statckTrace = exception.StackTrace ?? String.Empty;
            string message = exception.Message;
           
            var exceptionType = exception.GetType();

            if (exceptionType == typeof(DBConcurrencyException))
                status = HttpStatusCode.BadRequest;
            else if (exceptionType == typeof(NotFoundException))
                status = HttpStatusCode.NotFound;
            else
                status = HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new { status,message,statckTrace });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}