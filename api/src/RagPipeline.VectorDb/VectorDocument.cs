namespace RagPipeline.VectorDb;

public sealed record VectorDocument(string Id, string Text, float[] Embedding);

public sealed record VectorMatch(string Id, string Text, float Score);
