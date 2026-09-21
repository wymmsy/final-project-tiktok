namespace TikTokFeed.Engagement.Application.Abstractions.Services;

public sealed record UserExportView(
    Guid UserId,
    string Username,
    string Email,
    string? ProfileInfo,
    string? Avatar,
    DateTime? RegistrationDate,
    bool IsCreator,
    bool IsModerator,
    IReadOnlyList<Guid> FollowingIds,
    IReadOnlyList<Guid> FollowerIds);
