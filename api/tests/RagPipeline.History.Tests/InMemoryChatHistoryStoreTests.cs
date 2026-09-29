using RagPipeline.History;

namespace RagPipeline.History.Tests;

public class InMemoryChatHistoryStoreTests
{
    [Fact]
    public async Task List_is_empty_when_no_chat_has_been_saved()
    {
        var store = new InMemoryChatHistoryStore();

        var chats = await store.ListAsync();

        Assert.Empty(chats);
    }

    [Fact]
    public async Task Create_and_add_message_can_be_opened_again()
    {
        var store = new InMemoryChatHistoryStore();
        var chat = await store.CreateAsync("Ports");
        await store.AddMessageAsync(chat.Id, "user", "Why is the port in use?");
        await store.AddMessageAsync(chat.Id, "assistant", "Another process is listening on it.");

        var opened = await store.GetAsync(chat.Id);

        Assert.NotNull(opened);
        Assert.Equal("Ports", opened.Title);
        Assert.Equal(2, opened.Messages.Count);
        Assert.Equal("user", opened.Messages[0].Role);
    }

    [Fact]
    public async Task List_search_matches_the_title_or_a_message()
    {
        var store = new InMemoryChatHistoryStore();
        var ports = await store.CreateAsync("Port conflict");
        await store.AddMessageAsync(ports.Id, "user", "Check the listener.");
        await store.CreateAsync("Notebooks");

        var found = await store.ListAsync("listener");

        Assert.Equal(ports.Id, Assert.Single(found).Id);
    }
}
