namespace TikTokFeed.Engagement.Application.DTOs;

public sealed record CommentExportDto(Guid CommentId, Guid VideoId, string CommentText, DateTime CommentTimestamp, Guid? ParentCommentId);
