using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using devsu.DTOs;
using devsu.Exceptions;

namespace devsu.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                NotFoundException e => new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                    "No encontrado",
                    (int)HttpStatusCode.NotFound,
                    e.Message,
                    context.Request.Path
                ),
                KeyNotFoundException e => new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                    "No encontrado",
                    (int)HttpStatusCode.NotFound,
                    e.Message,
                    context.Request.Path
                ),
                BusinessException e => new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                    "Conflicto",
                    (int)HttpStatusCode.Conflict,
                    e.Message,
                    context.Request.Path
                ),
                InvalidOperationException e => new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                    "Conflicto",
                    (int)HttpStatusCode.Conflict,
                    e.Message,
                    context.Request.Path
                ),
                ArgumentException e => new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    "Solicitud inválida",
                    (int)HttpStatusCode.BadRequest,
                    e.Message,
                    context.Request.Path
                ),
                _ => new ErrorResponse(
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    "Error interno del servidor",
                    (int)HttpStatusCode.InternalServerError,
                    "Ocurrió un error al procesar la solicitud",
                    context.Request.Path
                )
            };

            context.Response.StatusCode = response.Status;

            if (response.Status == (int)HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Error no manejado: {Message}", exception.Message);
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}