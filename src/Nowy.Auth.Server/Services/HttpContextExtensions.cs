using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nowy.Auth.Server.Services;

public static class HttpContextExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static async ValueTask WriteAccessDeniedResponse(
        this HttpContext context,
        string? title = null,
        int? status_code = null,
        CancellationToken cancellation_token = default)
    {
        ProblemDetails problem = new ProblemDetails
        {
            Instance = context.Request.Path,
            Title = title ?? "Access denied",
            Status = status_code ?? StatusCodes.Status403Forbidden,
        };
        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonSerializerOptions), cancellation_token);
    }
}
