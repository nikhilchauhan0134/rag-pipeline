namespace RagPipeline.VectorDb;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, VectorDocument> _documents = new(StringComparer.Ordinal);

    public Task UpsertAsync(VectorDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            _documents[document.Id] = document;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorMatch>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        cancellationToken.ThrowIfCancellationRequested();

        if (topK <= 0)
        {
            return Task.FromResult<IReadOnlyList<VectorMatch>>([]);
        }

        List<VectorDocument> documents;
        lock (_gate)
        {
            documents = _documents.Values.ToList();
        }

        IReadOnlyList<VectorMatch> matches = documents
            .Select(document => new VectorMatch(document.Id, document.Text, Cosine(queryEmbedding, document.Embedding)))
            .OrderByDescending(match => match.Score)
            .Take(topK)
            .ToArray();

        return Task.FromResult(matches);
    }

    private static float Cosine(float[] left, float[] right)
    {
        if (left.Length == 0 || left.Length != right.Length)
        {
            return 0;
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;
        for (var index = 0; index < left.Length; index++)
        {
            dot += left[index] * right[index];
            leftNorm += left[index] * left[index];
            rightNorm += right[index] * right[index];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0;
        }

        return (float)(dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm)));
    }
}
