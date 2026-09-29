using RagPipeline.FineTuning;

namespace RagPipeline.FineTuning.Tests;

public class FineTuneExampleTests
{
    [Fact]
    public void ToJsonLine_writes_a_user_and_assistant_pair()
    {
        var line = FineTuneExample.ToJsonLine("Reset the password.", "Ask for the user id first.");

        var errors = FineTuneExample.Validate(line);

        Assert.Empty(errors);
        Assert.Contains("\"role\":\"user\"", line);
        Assert.Contains("\"role\":\"assistant\"", line);
    }

    [Fact]
    public void Validate_rejects_a_line_that_is_not_a_training_pair()
    {
        var errors = FineTuneExample.Validate("{\"messages\":[{\"role\":\"user\",\"content\":\"Hi\"}]}");

        Assert.Contains(errors, error => error.Contains("one user turn and one assistant turn"));
    }

    [Fact]
    public void Validate_rejects_empty_json()
    {
        var errors = FineTuneExample.Validate("  ");

        Assert.Contains(errors, error => error.Contains("empty"));
    }
}
