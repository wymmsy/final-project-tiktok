namespace TikTokFeed.Engagement.Application.DTOs;

public sealed record ContentExportDto(
    IReadOnlyList<VideoDto> Videos,
    IReadOnlyList<Guid> FavouriteVideoIds,
    IReadOnlyList<Guid> FavouriteSoundIds);
