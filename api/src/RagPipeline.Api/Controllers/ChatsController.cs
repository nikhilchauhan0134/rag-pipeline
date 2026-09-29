using Microsoft.AspNetCore.Mvc;
using RagPipeline.Api;
using RagPipeline.History;
using RagPipeline.Rag;

namespace RagPipeline.Api.Controllers;

[ApiController]
[Route("chats")]
public sealed class ChatsController : ControllerBase
{
    private readonly IChatHistoryStore _history;
    private readonly IRagPipeline _rag;
    private readonly IAnswerClient _answers;

    public ChatsController(IChatHistoryStore history, IRagPipeline rag, IAnswerClient answers)
    {
        _history = history;
        _rag = rag;
        _answers = answers;
    }

    [HttpPost]
    public async Task<ActionResult<ChatRecord>> Create([FromBody] CreateChatRequest? request, CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(request?.Title) ? "New chat" : request.Title.Trim();
        var chat = await _history.CreateAsync(title, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = chat.Id }, chat);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ChatRecord>>> List([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var chats = await _history.ListAsync(q, cancellationToken);
        return Ok(chats);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChatRecord>> Get(string id, CancellationToken cancellationToken)
    {
        var chat = await _history.GetAsync(id, cancellationToken);
        return chat is null ? NotFound() : Ok(chat);
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<SendMessageResponse>> Send(string id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { error = "Message content is required." });
        }

        var existing = await _history.GetAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        await _history.AddMessageAsync(id, "user", request.Content, cancellationToken);
        var prompt = await _rag.BuildAsync(request.Content, cancellationToken: cancellationToken);

        string answer;
        try
        {
            answer = await _answers.CompleteAsync(prompt.Text, request.Model ?? "Pro", cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            return Problem(title: "Groq request failed.", detail: exception.Message, statusCode: StatusCodes.Status502BadGateway);
        }

        var chat = await _history.AddMessageAsync(id, "assistant", answer, cancellationToken);
        return Ok(new SendMessageResponse(answer, chat));
    }
}
