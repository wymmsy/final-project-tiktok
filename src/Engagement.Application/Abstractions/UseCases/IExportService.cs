using TikTokFeed.Engagement.Application.DTOs;

namespace TikTokFeed.Engagement.Application.Abstractions.UseCases;

public interface IExportService
{
    Task<UserDataExportResponse> ExportUserDataAsync(Guid userId, CancellationToken cancellationToken = default);
}
