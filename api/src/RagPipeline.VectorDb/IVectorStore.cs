namespace RagPipeline.VectorDb;

public interface IVectorStore
{
    Task UpsertAsync(VectorDocument document, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorMatch>> SearchAsync(
        string chatId,
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default);
}
