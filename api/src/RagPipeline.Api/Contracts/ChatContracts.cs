using RagPipeline.History;

namespace RagPipeline.Api;

public sealed record CreateChatRequest(string? Title);

public sealed record SendMessageRequest(string Content, string? Model);

public sealed record SendMessageResponse(string Answer, ChatRecord Chat);
