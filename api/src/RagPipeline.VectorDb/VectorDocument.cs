namespace RagPipeline.VectorDb;

public sealed record VectorDocument(string Id, string ChatId, string Text, float[] Embedding);

public sealed record VectorMatch(string Id, string Text, float Score);
