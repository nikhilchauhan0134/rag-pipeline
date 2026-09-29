namespace RagPipeline.History;

public interface IChatHistoryStore
{
    Task<ChatRecord> CreateAsync(string title, CancellationToken cancellationToken = default);

    Task<ChatRecord> AddMessageAsync(
        string chatId,
        string role,
        string content,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatRecord>> ListAsync(string? search = null, CancellationToken cancellationToken = default);

    Task<ChatRecord?> GetAsync(string chatId, CancellationToken cancellationToken = default);
}
