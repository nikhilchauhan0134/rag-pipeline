using System.Security.Cryptography;
using System.Text;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace RagPipeline.VectorDb;

public sealed class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly string _collection;
    private readonly ulong _vectorSize;
    private readonly SemaphoreSlim _ready = new(1, 1);
    private bool _collectionReady;

    public QdrantVectorStore(string url, string apiKey, string collection, ulong vectorSize)
    {
        var host = new Uri(url).Host;
        _client = new QdrantClient(host, https: true, apiKey: apiKey);
        _collection = collection;
        _vectorSize = vectorSize;
    }

    public async Task UpsertAsync(VectorDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        await EnsureCollectionAsync(cancellationToken);

        var point = new PointStruct
        {
            Id = ToPointId(document.Id),
            Vectors = document.Embedding,
            Payload = { ["text"] = document.Text, ["documentId"] = document.Id, ["chatId"] = document.ChatId ?? "" },
        };

        await _client.UpsertAsync(_collection, new[] { point }, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<VectorMatch>> SearchAsync(
        string chatId,
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        if (topK <= 0)
        {
            return [];
        }

        await EnsureCollectionAsync(cancellationToken);
        
        var filter = new Filter
        {
            Must = { new Condition { Field = new FieldCondition { Key = "chatId", Match = new Match { Keyword = chatId ?? "" } } } }
        };

        var points = await _client.SearchAsync(
            _collection,
            queryEmbedding,
            filter: filter,
            limit: (ulong)topK,
            cancellationToken: cancellationToken);

        return points
            .Select(point =>
            {
                var id = point.Payload.TryGetValue("documentId", out var documentId)
                    ? documentId.StringValue
                    : point.Id.ToString();
                var text = point.Payload.TryGetValue("text", out var textValue) ? textValue.StringValue : "";
                return new VectorMatch(id, text, point.Score);
            })
            .ToArray();
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        if (_collectionReady)
        {
            return;
        }

        await _ready.WaitAsync(cancellationToken);
        try
        {
            if (_collectionReady)
            {
                return;
            }

            if (!await _client.CollectionExistsAsync(_collection, cancellationToken))
            {
                await _client.CreateCollectionAsync(
                    _collection,
                    new VectorParams { Size = _vectorSize, Distance = Distance.Cosine },
                    cancellationToken: cancellationToken);
                
                // Create index on chatId to allow keyword filtering
                await _client.CreatePayloadIndexAsync(
                    _collection,
                    "chatId",
                    PayloadSchemaType.Keyword,
                    cancellationToken: cancellationToken);
            }

            _collectionReady = true;
        }
        finally
        {
            _ready.Release();
        }
    }

    private static Guid ToPointId(string id)
    {
        if (Guid.TryParse(id, out var parsed))
        {
            return parsed;
        }

        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(id)));
    }
}
