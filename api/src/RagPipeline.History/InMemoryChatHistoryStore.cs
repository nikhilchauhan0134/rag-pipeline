namespace RagPipeline.History;

public sealed class InMemoryChatHistoryStore : IChatHistoryStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, ChatRecord> _chats = new(StringComparer.Ordinal);

    public Task<ChatRecord> CreateAsync(string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        cancellationToken.ThrowIfCancellationRequested();

        var chat = new ChatRecord(Guid.NewGuid().ToString("N"), title.Trim(), DateTimeOffset.UtcNow, []);
        lock (_gate)
        {
            _chats[chat.Id] = chat;
        }

        return Task.FromResult(chat);
    }

    public Task<ChatRecord> AddMessageAsync(
        string chatId,
        string role,
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            if (!_chats.TryGetValue(chatId, out var chat))
            {
                throw new KeyNotFoundException($"Chat '{chatId}' was not found.");
            }

            var messages = chat.Messages
                .Append(new ChatMessage(role.Trim(), content.Trim(), DateTimeOffset.UtcNow))
                .ToArray();
            var updated = chat with { UpdatedAt = DateTimeOffset.UtcNow, Messages = messages };
            _chats[chatId] = updated;
            return Task.FromResult(updated);
        }
    }

    public Task<IReadOnlyList<ChatRecord>> ListAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = search?.Trim();

        lock (_gate)
        {
            IEnumerable<ChatRecord> chats = _chats.Values;
            if (!string.IsNullOrWhiteSpace(query))
            {
                chats = chats.Where(chat =>
                    chat.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    chat.Messages.Any(message => message.Content.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            IReadOnlyList<ChatRecord> results = chats
                .OrderByDescending(chat => chat.UpdatedAt)
                .ToArray();
            return Task.FromResult(results);
        }
    }

    public Task<ChatRecord?> GetAsync(string chatId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _chats.TryGetValue(chatId, out var chat);
            return Task.FromResult(chat);
        }
    }
}
