using Grpc.Core;
using TikTokFeed.Contracts.Grpc.Identity;
using TikTokFeed.Engagement.Application.Abstractions.Services;

namespace TikTokFeed.Engagement.Infrastructure.Grpc;

public class IdentityGateway : IIdentityGateway
{
    private readonly IdentityService.IdentityServiceClient _client;

    public IdentityGateway(IdentityService.IdentityServiceClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Guid>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken)
    {
        FollowingProto response = await _client.GetFollowingAsync(
            new GetFollowingRequest { UserId = userId.ToString() }, cancellationToken: cancellationToken);

        var result = new List<Guid>(response.FollowingIds.Count);
        foreach (string id in response.FollowingIds)
        {
            if (Guid.TryParse(id, out Guid parsed))
            {
                result.Add(parsed);
            }
        }

        return result;
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken)
    {
        // Намеренный явный кросс-сервисный вызов проверки токена (Этап 4, §5.1 trade-off).
        TokenValidationProto response = await _client.ValidateTokenAsync(
            new ValidateTokenRequest { Token = token }, cancellationToken: cancellationToken);
        return response.IsValid;
    }

    public async Task<UserView?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            UserProto user = await _client.GetUserAsync(
                new GetUserRequest { UserId = userId.ToString() }, cancellationToken: cancellationToken);
            return new UserView(Guid.Parse(user.UserId), user.Username, user.IsCreator);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<UserExportView?> GetUserExportDataAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            UserExportDataProto data = await _client.GetUserExportDataAsync(
                new GetUserExportDataRequest { UserId = userId.ToString() }, cancellationToken: cancellationToken);

            return new UserExportView(
                Guid.Parse(data.UserId),
                data.Username,
                data.Email,
                string.IsNullOrEmpty(data.ProfileInfo) ? null : data.ProfileInfo,
                string.IsNullOrEmpty(data.Avatar) ? null : data.Avatar,
                string.IsNullOrEmpty(data.RegistrationDate) ? null : DateTime.Parse(data.RegistrationDate, null, System.Globalization.DateTimeStyles.RoundtripKind),
                data.IsCreator,
                data.IsModerator,
                data.FollowingIds.Select(Guid.Parse).ToList(),
                data.FollowerIds.Select(Guid.Parse).ToList());
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
