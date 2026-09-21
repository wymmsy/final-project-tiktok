using TikTokFeed.Content.Domain.Entities;

namespace TikTokFeed.Content.Application.Abstractions.Repositories;

public interface IFavouriteRepository
{
    Task<bool> SoundExistsAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default);

    Task<FavouriteSound?> GetSoundAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetFavouriteSoundIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    void AddSound(FavouriteSound favourite);

    void RemoveSound(FavouriteSound favourite);
}
