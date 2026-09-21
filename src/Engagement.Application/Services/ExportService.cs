using TikTokFeed.Engagement.Application.Abstractions.Repositories;
using TikTokFeed.Engagement.Application.Abstractions.Services;
using TikTokFeed.Engagement.Application.Abstractions.UseCases;
using TikTokFeed.Engagement.Application.DTOs;
using TikTokFeed.Engagement.Application.Mappings;
using TikTokFeed.Engagement.Domain.Entities;
using TikTokFeed.Engagement.Domain.Exceptions;

namespace TikTokFeed.Engagement.Application.Services;

// Собирает данные пользователя из своей БД и других сервисов (Identity, Content) в один self-service экспорт
public class ExportService : IExportService
{
    private readonly ILikeRepository _likes;

    private readonly ICommentRepository _comments;

    private readonly IRepostRepository _reposts;

    private readonly IViewRepository _views;

    private readonly ICurrentUserService _currentUser;

    private readonly IIdentityGateway _identity;

    private readonly IContentGateway _content;

    public ExportService(
        ILikeRepository likes,
        ICommentRepository comments,
        IRepostRepository reposts,
        IViewRepository views,
        ICurrentUserService currentUser,
        IIdentityGateway identity,
        IContentGateway content)
    {
        _likes = likes;
        _comments = comments;
        _reposts = reposts;
        _views = views;
        _currentUser = currentUser;
        _identity = identity;
        _content = content;
    }

    public async Task<UserDataExportResponse> ExportUserDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId != userId)
        {
            throw new ForbiddenException("You can only export your own data");
        }

        UserExportView identity = await _identity.GetUserExportDataAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found");

        ContentExportView content = await _content.GetUserExportDataAsync(userId, cancellationToken);

        IReadOnlyList<Like> likes = await _likes.GetByUserAsync(userId, cancellationToken);
        IReadOnlyList<Comment> comments = await _comments.GetByUserAsync(userId, cancellationToken);
        IReadOnlyList<Repost> reposts = await _reposts.GetByUserAsync(userId, cancellationToken);
        IReadOnlyList<View> views = await _views.GetByUserAsync(userId, cancellationToken);

        return new UserDataExportResponse(
            userId,
            DateTime.UtcNow,
            new IdentityExportDto(
                identity.UserId,
                identity.Username,
                identity.Email,
                identity.ProfileInfo,
                identity.Avatar,
                identity.RegistrationDate,
                identity.IsCreator,
                identity.IsModerator,
                identity.FollowingIds,
                identity.FollowerIds),
            new ContentExportDto(
                content.Videos.Select(video => video.ToDto()).ToList(),
                content.FavouriteVideoIds,
                content.FavouriteSoundIds),
            new EngagementExportDto(
                likes.Select(like => like.ToExportDto()).ToList(),
                comments.Select(comment => comment.ToExportDto()).ToList(),
                reposts.Select(repost => repost.ToExportDto()).ToList(),
                views.Select(view => view.ToExportDto()).ToList()));
    }
}
