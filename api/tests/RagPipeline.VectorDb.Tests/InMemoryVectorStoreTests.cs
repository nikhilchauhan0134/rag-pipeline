using RagPipeline.VectorDb;

namespace RagPipeline.VectorDb.Tests;

public class InMemoryVectorStoreTests
{
    [Fact]
    public async Task Search_returns_the_nearest_document()
    {
        var store = new InMemoryVectorStore();
        await store.UpsertAsync(new VectorDocument("ports", "chat-1", "A port conflict means another process is listening.", [0, 1]));
        await store.UpsertAsync(new VectorDocument("rag", "chat-1", "RAG retrieves passages before the model answers.", [1, 0]));

        var matches = await store.SearchAsync("chat-1", [1, 0], topK: 1);

        var match = Assert.Single(matches);
        Assert.Equal("rag", match.Id);
        Assert.Contains("RAG retrieves", match.Text);
    }

    [Fact]
    public async Task Upsert_replaces_a_document_with_the_same_id()
    {
        var store = new InMemoryVectorStore();
        await store.UpsertAsync(new VectorDocument("rag", "chat-1", "old text", [1, 0]));
        await store.UpsertAsync(new VectorDocument("rag", "chat-1", "new text", [1, 0]));

        var matches = await store.SearchAsync("chat-1", [1, 0], topK: 1);

        Assert.Equal("new text", Assert.Single(matches).Text);
    }
}
