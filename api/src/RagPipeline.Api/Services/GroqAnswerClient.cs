using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RagPipeline.Api;

public interface IAnswerClient
{
    Task<string> CompleteAsync(string prompt, string model, CancellationToken cancellationToken);
}

public sealed class GroqAnswerClient(HttpClient http, IConfiguration configuration) : IAnswerClient
{
    public async Task<string> CompleteAsync(string prompt, string model, CancellationToken cancellationToken)
    {
        var apiKey = configuration["Groq:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "The Groq API key is not set. Add GROQ_API_KEY to receive a model answer.";
        }

        var modelName = string.Equals(model, "Fast", StringComparison.OrdinalIgnoreCase)
            ? configuration["Groq:FastModel"] ?? "openai/gpt-oss-20b"
            : configuration["Groq:ProModel"] ?? "openai/gpt-oss-120b";

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = modelName,
            stream = false,
            messages = new[] { new { role = "user", content = prompt } },
        });

        var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<GroqResponse>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken);
        var text = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        return string.IsNullOrWhiteSpace(text) ? "Groq returned an empty answer." : text;
    }

    private sealed class GroqResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }

    private sealed class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private sealed class GroqMessage
    {
        public string? Content { get; set; }
    }
}
