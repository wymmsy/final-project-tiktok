namespace TikTokFeed.Engagement.Application.DTOs;

public sealed record IdentityExportDto(
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
