using RagPipeline.Rag;

namespace RagPipeline.Api;

public sealed class HashEmbeddingGenerator : IEmbeddingGenerator
{
    public float[] Embed(string text)
    {
        var vector = new float[8];
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(word);
            vector[(hash & int.MaxValue) % vector.Length] += 1;
        }

        return vector;
    }
}
