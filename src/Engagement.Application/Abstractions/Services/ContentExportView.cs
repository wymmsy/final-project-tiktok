namespace TikTokFeed.Engagement.Application.Abstractions.Services;

public sealed record ContentExportView(
    IReadOnlyList<VideoView> Videos,
    IReadOnlyList<Guid> FavouriteVideoIds,
    IReadOnlyList<Guid> FavouriteSoundIds);
