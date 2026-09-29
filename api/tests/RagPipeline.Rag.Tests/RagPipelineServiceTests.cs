using RagPipeline.Rag;
using RagPipeline.VectorDb;

namespace RagPipeline.Rag.Tests;

public class RagPipelineServiceTests
{
    [Fact]
    public async Task Build_puts_the_nearest_passage_and_the_question_in_the_prompt()
    {
        var vectors = new InMemoryVectorStore();
        await vectors.UpsertAsync(new VectorDocument("rag", "chat-1", "RAG supplies factual memory from retrieved passages.", [1, 0]));
        await vectors.UpsertAsync(new VectorDocument("other", "chat-1", "This passage is about something else.", [0, 1]));
        var pipeline = new RagPipelineService(new FixedEmbeddingGenerator([1, 0]), vectors);

        var prompt = await pipeline.BuildAsync("chat-1", "What does RAG supply?", topK: 1);

        Assert.Equal("What does RAG supply?", prompt.Question);
        Assert.Contains("factual memory", prompt.Text);
        Assert.DoesNotContain("something else", prompt.Text);
    }

    [Fact]
    public async Task Build_still_returns_a_prompt_when_the_store_is_empty()
    {
        var pipeline = new RagPipelineService(new FixedEmbeddingGenerator([1, 0]), new InMemoryVectorStore());

        var prompt = await pipeline.BuildAsync("chat-1", "Hello");

        Assert.Contains("Question: Hello", prompt.Text);
        Assert.Contains("(none)", prompt.Text);
        Assert.Empty(prompt.Passages);
    }

    private sealed class FixedEmbeddingGenerator(float[] embedding) : IEmbeddingGenerator
    {
        public float[] Embed(string text) => embedding;
    }
}
