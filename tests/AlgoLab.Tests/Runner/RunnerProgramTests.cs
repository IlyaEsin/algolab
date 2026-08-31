using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Running;
using AlgoLab.Runner;
using AlgoLab.Tests.Fakes;

namespace AlgoLab.Tests.Runner;

public sealed class RunnerProgramTests
{
    // Guards the fix for finding 2: a solution that prints to the console (an ordinary thing to
    // do while debugging an attempt) must not corrupt the JSON payload on the same stdout stream.
    // RunnerProgram is expected to redirect Console.Out/Error to TextWriter.Null for the duration
    // of RunnerCommand.Execute and restore them afterwards.
    [Fact]
    public void A_solution_that_prints_to_console_does_not_corrupt_the_payload()
    {
        var writer = new StringWriter();

        var exitCode = RunnerProgram.Run(
            ["cases", "--slug", "fake-loud", "--solution", nameof(LoudDoubles)],
            FakeRegistry.Instance,
            writer);

        Assert.Equal(0, exitCode);

        var output = writer.ToString();
        var payload = JsonSerializer.Deserialize<RunnerPayload>(output, AlgoLabJson.Options);

        Assert.NotNull(payload);
        Assert.Null(payload!.Error);
        Assert.Equal(RunStatus.Passed, payload.Run!.Status);
        Assert.DoesNotContain("corrupt", output, StringComparison.Ordinal);
        Assert.DoesNotContain("шум", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Console_out_and_error_are_restored_after_running()
    {
        var writer = new StringWriter();

        RunnerProgram.Run(
            ["cases", "--slug", "fake-loud", "--solution", nameof(LoudDoubles)],
            FakeRegistry.Instance,
            writer);

        Assert.NotSame(TextWriter.Null, Console.Out);
        Assert.NotSame(TextWriter.Null, Console.Error);
    }
}
