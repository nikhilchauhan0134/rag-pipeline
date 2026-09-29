using System.Text;
using RagPipeline.VectorDb;

namespace RagPipeline.Rag;

public sealed class RagPipelineService : IRagPipeline
{
    private readonly IEmbeddingGenerator _embeddings;
    private readonly IVectorStore _vectors;

    public RagPipelineService(IEmbeddingGenerator embeddings, IVectorStore vectors)
    {
        _embeddings = embeddings;
        _vectors = vectors;
    }

    public async Task<RagPrompt> BuildAsync(string chatId, string question, int topK = 3, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        var embedding = _embeddings.Embed(question);
        var passages = await _vectors.SearchAsync(chatId, embedding, topK, cancellationToken);
        return new RagPrompt(question.Trim(), passages, WritePrompt(question.Trim(), passages));
    }

    private static string WritePrompt(string question, IReadOnlyList<VectorMatch> passages)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("You are a helpful and intelligent AI assistant.");
        prompt.AppendLine("Use the provided contextual Passages to help answer the user's Question or fulfill their request.");
        prompt.AppendLine("If the user asks you to do something outside your capabilities (like generating a file) or asks a question not covered by the context, respond naturally and politely. Do NOT awkwardly say 'Based on the passages provided...'. Just talk to the user like a normal assistant.");
        prompt.AppendLine();
        prompt.AppendLine("Passages:");
        if (passages.Count == 0)
        {
            prompt.AppendLine("(none)");
        }
        else
        {
            foreach (var passage in passages)
            {
                prompt.Append("- ").AppendLine(passage.Text);
            }
        }

        prompt.AppendLine();
        prompt.Append("Question: ").Append(question);
        return prompt.ToString();
    }
}
