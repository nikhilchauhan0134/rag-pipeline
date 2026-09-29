using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RagPipeline.Api.Tests;

public class ChatApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ChatApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task List_is_empty_for_a_new_host()
    {
        await using var factory = new ApiFactory();
        var chats = await factory.CreateClient().GetFromJsonAsync<JsonElement>("/chats");

        Assert.Equal(JsonValueKind.Array, chats.ValueKind);
        Assert.Equal(0, chats.GetArrayLength());
    }

    [Fact]
    public async Task Send_stores_the_question_and_returns_the_full_answer()
    {
        var client = _factory.CreateClient();
        var created = await client.PostAsJsonAsync("/chats", new { title = "Ports" });
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetString();

        var sent = await client.PostAsJsonAsync($"/chats/{id}/messages", new { content = "Why is the port busy?", model = "Fast" });
        var body = await sent.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, sent.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("answer").GetString()));
        Assert.Equal(2, body.GetProperty("chat").GetProperty("messages").GetArrayLength());
    }

    [Fact]
    public async Task Send_to_a_missing_chat_returns_not_found()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/chats/missing/messages",
            new { content = "Hello", model = "Pro" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Send_without_content_returns_bad_request()
    {
        var client = _factory.CreateClient();
        var created = await client.PostAsJsonAsync("/chats", new { title = "Empty" });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();

        var response = await client.PostAsJsonAsync($"/chats/{id}/messages", new { content = "  ", model = "Pro" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
