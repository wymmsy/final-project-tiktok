using Grpc.Core;
using TikTokFeed.Content.Application.Abstractions.Repositories;
using TikTokFeed.Content.Domain.Entities;
using TikTokFeed.Contracts.Grpc.Content;

namespace TikTokFeed.Content.Api.Grpc;

// gRPC-поверхность Content
public class ContentGrpcService : ContentService.ContentServiceBase
{
    private readonly IVideoRepository _videos;

    private readonly IFavouriteRepository _favourites;

    public ContentGrpcService(IVideoRepository videos, IFavouriteRepository favourites)
    {
        _videos = videos;
        _favourites = favourites;
    }

    public override async Task<VideoProto> GetVideo(GetVideoRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (!Guid.TryParse(request.VideoId, out Guid id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Bad video_id"));
        }

        Video video = await _videos.GetByIdAsync(id, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Video not found"));

        return Map(video);
    }

    public override async Task<VideosProto> GetVideosByUser(GetVideosByUserRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var result = new VideosProto();
        if (!Guid.TryParse(request.UserId, out Guid userId))
        {
            return result;
        }

        IReadOnlyList<Video> videos = await _videos.GetByUserAsync(userId, context.CancellationToken);
        result.Videos.AddRange(videos.Select(Map));
        return result;
    }

    public override async Task<ContentExportDataProto> GetUserExportData(GetUserExportDataRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var result = new ContentExportDataProto();
        if (!Guid.TryParse(request.UserId, out Guid userId))
        {
            return result;
        }

        IReadOnlyList<Video> videos = await _videos.GetByUserAsync(userId, context.CancellationToken);
        IReadOnlyList<Guid> favouriteVideoIds = await _favourites.GetFavouriteVideoIdsAsync(userId, context.CancellationToken);
        IReadOnlyList<Guid> favouriteSoundIds = await _favourites.GetFavouriteSoundIdsAsync(userId, context.CancellationToken);

        result.Videos.AddRange(videos.Select(Map));
        result.FavouriteVideoIds.AddRange(favouriteVideoIds.Select(id => id.ToString()));
        result.FavouriteSoundIds.AddRange(favouriteSoundIds.Select(id => id.ToString()));

        return result;
    }

    private static VideoProto Map(Video video) => new()
    {
        VideoId = video.Id.ToString(),
        UserId = video.UserId.ToString(),
        Caption = video.Caption,
        IsPublic = video.IsPublic,
        ModerationStatus = video.ModerationStatus.ToString().ToUpperInvariant(),
        ProcessingStatus = video.ProcessingStatus.ToString().ToUpperInvariant(),
    };
}
