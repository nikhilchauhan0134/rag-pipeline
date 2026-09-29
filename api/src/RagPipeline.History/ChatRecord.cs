namespace RagPipeline.History;

public sealed record ChatMessage(string Role, string Content, DateTimeOffset At);

public sealed record ChatRecord(
    string Id,
    string Title,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ChatMessage> Messages);
