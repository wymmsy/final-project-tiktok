using TikTokFeed.Content.Application.DTOs;

namespace TikTokFeed.Content.Application.Abstractions.UseCases;

public interface IFavouriteService
{
    Task<IReadOnlyList<SoundResponse>> ListSoundsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddSoundAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default);

    Task RemoveSoundAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default);
}
