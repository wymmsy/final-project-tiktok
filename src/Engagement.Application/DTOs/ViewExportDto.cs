namespace TikTokFeed.Engagement.Application.DTOs;

public sealed record ViewExportDto(Guid VideoId, DateTime ViewTimestamp, int WatchDuration);
