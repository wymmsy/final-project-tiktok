using TikTokFeed.Content.Application.Abstractions.Repositories;
using TikTokFeed.Content.Application.Abstractions.Services;
using TikTokFeed.Content.Application.Abstractions.UseCases;
using TikTokFeed.Content.Application.DTOs;
using TikTokFeed.Content.Application.Mappings;
using TikTokFeed.Content.Domain.Entities;
using TikTokFeed.Content.Domain.Exceptions;

namespace TikTokFeed.Content.Application.Services;

public class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _favourites;

    private readonly ISoundRepository _sounds;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUserService _currentUser;

    public FavouriteService(
        IFavouriteRepository favourites,
        ISoundRepository sounds,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _favourites = favourites;
        _sounds = sounds;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SoundResponse>> ListSoundsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureSelf(userId);
        IReadOnlyList<Guid> ids = await _favourites.GetFavouriteSoundIdsAsync(userId, cancellationToken);
        IReadOnlyList<Sound> sounds = await _sounds.GetByIdsAsync(ids, cancellationToken);
        return sounds.Select(sound => sound.ToResponse()).ToList();
    }

    public async Task AddSoundAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default)
    {
        EnsureSelf(userId);
        if (!await _sounds.ExistsAsync(soundId, cancellationToken))
        {
            throw new NotFoundException("Sound not found");
        }

        if (await _favourites.SoundExistsAsync(userId, soundId, cancellationToken))
        {
            throw new ConflictException("ALREADY_FAVOURITE", "Sound already in favourites");
        }

        _favourites.AddSound(new FavouriteSound(userId, soundId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSoundAsync(Guid userId, Guid soundId, CancellationToken cancellationToken = default)
    {
        EnsureSelf(userId);
        FavouriteSound favourite = await _favourites.GetSoundAsync(userId, soundId, cancellationToken)
            ?? throw new NotFoundException("Not found in favourites");

        _favourites.RemoveSound(favourite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureSelf(Guid userId)
    {
        if (_currentUser.UserId != userId)
        {
            throw new ForbiddenException("You can only manage your own favourites");
        }
    }
}
