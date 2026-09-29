using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RagPipeline.Api;


public sealed class OpenAIAnswerClient(HttpClient http, IConfiguration configuration) : IAnswerClient
{
    public async Task<string> CompleteAsync(string prompt, string model, CancellationToken cancellationToken)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "The OpenAI API key is not set. Add OPENAI_API_KEY to receive a model answer.";
        }

        var modelName = string.Equals(model, "Fast", StringComparison.OrdinalIgnoreCase)
            ? configuration["OpenAI:FastModel"] ?? "gpt-4o-mini"
            : configuration["OpenAI:ProModel"] ?? "gpt-4o";

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = modelName,
            messages = new[] { new { role = "user", content = prompt } },
        });

        var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<OpenAIResponse>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken);
        var text = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        return string.IsNullOrWhiteSpace(text) ? "OpenAI returned an empty answer." : text;
    }

    private sealed class OpenAIResponse
    {
        public List<OpenAIChoice>? Choices { get; set; }
    }

    private sealed class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
    }

    private sealed class OpenAIMessage
    {
        public string? Content { get; set; }
    }
}
