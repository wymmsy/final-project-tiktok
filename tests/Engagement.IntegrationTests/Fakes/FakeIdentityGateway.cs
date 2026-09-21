using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TikTokFeed.Engagement.Application.Abstractions.Services;

namespace TikTokFeed.Engagement.IntegrationTests.Fakes;

public sealed class FakeIdentityGateway : IIdentityGateway
{
    public Dictionary<Guid, List<Guid>> Following { get; } = new();

    public Dictionary<Guid, List<Guid>> Followers { get; } = new();

    public Dictionary<Guid, UserView> Users { get; } = new();

    public Dictionary<Guid, UserExportView> ExportData { get; } = new();

    public Task<IReadOnlyList<Guid>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult((IReadOnlyList<Guid>)(Following.TryGetValue(userId, out List<Guid>? list) ? list : new List<Guid>()));

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task<UserView?> GetUserAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.TryGetValue(userId, out UserView? user) ? user : null);

    public Task<UserExportView?> GetUserExportDataAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(ExportData.TryGetValue(userId, out UserExportView? data) ? data : BuildDefault(userId));

    private UserExportView? BuildDefault(Guid userId)
    {
        if (!Users.TryGetValue(userId, out UserView? user))
        {
            return null;
        }

        return new UserExportView(
            user.UserId,
            user.Username,
            $"{user.Username}@example.com",
            null,
            null,
            DateTime.UtcNow,
            user.IsCreator,
            false,
            Following.TryGetValue(userId, out List<Guid>? following) ? following : new List<Guid>(),
            Followers.TryGetValue(userId, out List<Guid>? followers) ? followers : new List<Guid>());
    }
}
