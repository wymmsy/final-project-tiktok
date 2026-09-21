using Microsoft.EntityFrameworkCore;
using TikTokFeed.Content.Application.Abstractions.Repositories;
using TikTokFeed.Content.Domain.Entities;

namespace TikTokFeed.Content.Infrastructure.Persistence.Repositories;

public class FavouriteRepository : IFavouriteRepository
{
    private readonly ContentDbContext _context;

    public FavouriteRepository(ContentDbContext context)
    {
        _context = context;
    }

    public Task<bool> SoundExistsAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default) =>
        _context.FavouriteSounds.AnyAsync(f => f.UserId == userId && f.SoundId == soundId, cancellationToken);

    public Task<FavouriteSound?> GetSoundAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default) =>
        _context.FavouriteSounds.FirstOrDefaultAsync(f => f.UserId == userId && f.SoundId == soundId, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetFavouriteSoundIdsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.FavouriteSounds.Where(f => f.UserId == userId).Select(f => f.SoundId).ToListAsync(cancellationToken);

    public void AddSound(FavouriteSound favourite) => _context.FavouriteSounds.Add(favourite);

    public void RemoveSound(FavouriteSound favourite) => _context.FavouriteSounds.Remove(favourite);
}
