using TikTokFeed.Engagement.Application.Abstractions.Services;
using TikTokFeed.Engagement.Application.DTOs;
using TikTokFeed.Engagement.Domain.Entities;

namespace TikTokFeed.Engagement.Application.Mappings;

public static class ExportMappingExtensions
{
    public static VideoDto ToDto(this VideoView video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return new VideoDto(
            video.VideoId,
            video.UserId,
            video.Caption,
            video.VideoUrl,
            video.ThumbnailUrl,
            video.Duration,
            video.Resolution,
            video.UploadTimestamp,
            video.IsPublic,
            video.SoundId,
            video.ModerationStatus,
            video.ProcessingStatus);
    }

    public static LikeExportDto ToExportDto(this Like like)
    {
        ArgumentNullException.ThrowIfNull(like);

        return new LikeExportDto(like.VideoId, like.LikeTimestamp);
    }

    public static CommentExportDto ToExportDto(this Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        return new CommentExportDto(
            comment.Id,
            comment.VideoId,
            comment.CommentText,
            comment.CommentTimestamp,
            comment.ParentCommentId);
    }

    public static RepostExportDto ToExportDto(this Repost repost)
    {
        ArgumentNullException.ThrowIfNull(repost);

        return new RepostExportDto(repost.VideoId, repost.RepostTimestamp);
    }

    public static ViewExportDto ToExportDto(this View view)
    {
        ArgumentNullException.ThrowIfNull(view);

        return new ViewExportDto(view.VideoId, view.ViewTimestamp, view.WatchDuration);
    }
}
