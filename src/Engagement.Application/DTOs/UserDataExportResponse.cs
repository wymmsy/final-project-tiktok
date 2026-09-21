namespace TikTokFeed.Engagement.Application.DTOs;

public sealed record UserDataExportResponse(
    Guid UserId,
    DateTime ExportedAt,
    IdentityExportDto Identity,
    ContentExportDto Content,
    EngagementExportDto Engagement);
