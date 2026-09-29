using RagPipeline.VectorDb;

namespace RagPipeline.Rag;

public interface IEmbeddingGenerator
{
    float[] Embed(string text);
}

public sealed record RagPrompt(string Question, IReadOnlyList<VectorMatch> Passages, string Text);

public interface IRagPipeline
{
    Task<RagPrompt> BuildAsync(string chatId, string question, int topK = 3, CancellationToken cancellationToken = default);
}
