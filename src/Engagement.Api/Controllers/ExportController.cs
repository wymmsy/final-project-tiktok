using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TikTokFeed.Engagement.Application.Abstractions.UseCases;
using TikTokFeed.Engagement.Application.DTOs;

namespace TikTokFeed.Engagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/users/{userId:guid}/export")]
[Produces("application/json")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    private readonly JsonSerializerOptions _jsonOptions;

    public ExportController(IExportService exportService, IOptions<JsonOptions> jsonOptions)
    {
        _exportService = exportService;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(Guid userId, CancellationToken cancellationToken)
    {
        UserDataExportResponse data = await _exportService.ExportUserDataAsync(userId, cancellationToken);

        byte[] content = JsonSerializer.SerializeToUtf8Bytes(data, _jsonOptions);
        string fileName = $"user-data-export-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";

        return File(content, "application/json", fileName);
    }
}
