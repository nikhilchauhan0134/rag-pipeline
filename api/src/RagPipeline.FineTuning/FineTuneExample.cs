using System.Text.Json;

namespace RagPipeline.FineTuning;

public static class FineTuneExample
{
    public static string ToJsonLine(string user, string assistant)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(assistant);

        var line = new
        {
            messages = new[]
            {
                new { role = "user", content = user.Trim() },
                new { role = "assistant", content = assistant.Trim() },
            },
        };

        return JsonSerializer.Serialize(line);
    }

    public static IReadOnlyList<string> Validate(string jsonLine)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(jsonLine))
        {
            errors.Add("The example line is empty.");
            return errors;
        }

        try
        {
            using var document = JsonDocument.Parse(jsonLine);
            if (!document.RootElement.TryGetProperty("messages", out var messages) ||
                messages.ValueKind != JsonValueKind.Array)
            {
                errors.Add("The example needs a messages array.");
                return errors;
            }

            var turns = messages.EnumerateArray().ToArray();
            if (turns.Length != 2)
            {
                errors.Add("The example needs one user turn and one assistant turn.");
                return errors;
            }

            RequireTurn(turns[0], "user", errors);
            RequireTurn(turns[1], "assistant", errors);
        }
        catch (JsonException)
        {
            errors.Add("The example is not valid JSON.");
        }

        return errors;
    }

    private static void RequireTurn(JsonElement turn, string role, List<string> errors)
    {
        var actualRole = turn.TryGetProperty("role", out var roleElement) ? roleElement.GetString() : null;
        var content = turn.TryGetProperty("content", out var contentElement) ? contentElement.GetString() : null;
        if (!string.Equals(actualRole, role, StringComparison.Ordinal))
        {
            errors.Add($"Expected the {role} role.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            errors.Add($"The {role} content is empty.");
        }
    }
}
