using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TikTokFeed.Engagement.Application.Abstractions.Services;
using TikTokFeed.Engagement.IntegrationTests.Infrastructure;
using Xunit;

namespace TikTokFeed.Engagement.IntegrationTests;

public sealed class ExportTests : IClassFixture<EngagementTestFactory>
{
    private readonly EngagementTestFactory _factory;

    public ExportTests(EngagementTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Export_Self_ReturnsAggregatedData()
    {
        var user = Guid.NewGuid();
        _factory.Identity.Users[user] = new UserView(user, "exporter", false);
        VideoView video = _factory.Content.AddApproved(owner: user);
        HttpClient client = _factory.CreateClientFor(user, "exporter");

        HttpResponseMessage likeResponse = await client.PostAsync($"/api/v1/videos/{video.VideoId}/likes", null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/{user}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentDisposition.Should().NotBeNull();
        JsonElement root = await TestJson.Root(response);
        root.GetProperty("user_id").GetGuid().Should().Be(user);
        root.GetProperty("identity").GetProperty("username").GetString().Should().Be("exporter");
        root.GetProperty("content").GetProperty("videos").GetArrayLength().Should().Be(1);
        root.GetProperty("engagement").GetProperty("likes").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task Export_OtherUsersData_ReturnsForbidden()
    {
        var owner = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        _factory.Identity.Users[owner] = new UserView(owner, "owner", false);
        HttpClient client = _factory.CreateClientFor(stranger, "stranger");

        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/{owner}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Export_MissingToken_ReturnsUnauthorized()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/{Guid.NewGuid()}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Export_UnknownUser_ReturnsNotFound()
    {
        var user = Guid.NewGuid();
        HttpClient client = _factory.CreateClientFor(user, "ghost");

        HttpResponseMessage response = await client.GetAsync($"/api/v1/users/{user}/export");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
