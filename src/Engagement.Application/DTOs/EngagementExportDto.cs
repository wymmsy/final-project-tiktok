namespace TikTokFeed.Engagement.Application.DTOs;

public sealed record EngagementExportDto(
    IReadOnlyList<LikeExportDto> Likes,
    IReadOnlyList<CommentExportDto> Comments,
    IReadOnlyList<RepostExportDto> Reposts,
    IReadOnlyList<ViewExportDto> Views);
