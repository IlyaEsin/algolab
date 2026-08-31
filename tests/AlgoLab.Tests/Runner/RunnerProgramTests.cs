using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Running;
using AlgoLab.Runner;
using AlgoLab.Tests.Fakes;

// This suite points Console.Out/Console.Error at the same writer RunnerProgram receives as
// `output`, to reproduce the real coupling: in the spawned process both the payload and any
// stray Console.WriteLine from a solution share one OS stdout handle. Mutating the process-wide
// Console.Out/Error is safe only if nothing else in the assembly can observe it mid-test —
// xUnit parallelises different test classes against each other by default, and a per-collection
// DisableParallelization only serialises tests *within* that collection, not across collections.
// Disabling parallelisation for the whole assembly is the only mechanism that actually
// guarantees no other test's thread can be running while Console.Out is redirected here.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AlgoLab.Tests.Runner;

public sealed class RunnerProgramTests
{
    // Guards the fix for finding 2: a solution that prints to the console (an ordinary thing to
    // do while debugging an attempt) must not corrupt the JSON payload on the same stdout stream.
    //
    // Forcing by construction: Console.Out/Error are set to the very same StringWriter passed as
    // `output` before calling RunnerProgram.Run. If RunnerProgram's redirect
    // (Console.SetOut/SetError to TextWriter.Null around RunnerCommand.Execute) is ever removed,
    // LoudDoubles' prints land in that same buffer as the payload and the JSON below fails to
    // deserialize. With the redirect in place, the buffer holds only the payload.
    [Fact]
    public void A_solution_that_prints_to_console_does_not_corrupt_the_payload()
    {
        var writer = new StringWriter();
        var originalOut = Console.Out;
        var originalError = Console.Error;
        int exitCode;

        try
        {
            Console.SetOut(writer);
            Console.SetError(writer);

            exitCode = RunnerProgram.Run(
                ["cases", "--slug", "fake-loud", "--solution", nameof(LoudDoubles)],
                FakeRegistry.Instance,
                writer);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        Assert.Equal(0, exitCode);

        var output = writer.ToString();
        var payload = JsonSerializer.Deserialize<RunnerPayload>(output, AlgoLabJson.Options);

        Assert.NotNull(payload);
        Assert.Null(payload!.Error);
        Assert.Equal(RunStatus.Passed, payload.Run!.Status);
        Assert.DoesNotContain("corrupt", output, StringComparison.Ordinal);
        Assert.DoesNotContain("шум", output, StringComparison.Ordinal);
    }

    // Forcing by construction: Console.Out/Error are pointed at `writer` before the call, exactly
    // like production would see them mid-way through (RunnerProgram redirects them to
    // TextWriter.Null internally). Console.SetOut wraps whatever it is given in a synchronizing
    // TextWriter, so the getter never returns the exact same reference back — reference equality
    // can't tell "restored to writer" apart from "restored to something else". Writing a marker
    // through Console.Out/Error after the call can: if RunnerProgram's `finally` restore is ever
    // removed, Console.Out/Error are still TextWriter.Null here, the marker is silently
    // discarded, and it never reaches the buffer.
    [Fact]
    public void Console_out_and_error_are_restored_to_the_caller_s_writer_after_running()
    {
        var writer = new StringWriter();
        var originalOut = Console.Out;
        var originalError = Console.Error;

        try
        {
            Console.SetOut(writer);
            Console.SetError(writer);

            RunnerProgram.Run(
                ["cases", "--slug", "fake-loud", "--solution", nameof(LoudDoubles)],
                FakeRegistry.Instance,
                writer);

            Console.Out.Write("OUT-RESTORED");
            Console.Error.Write("ERROR-RESTORED");
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        var buffer = writer.ToString();
        Assert.Contains("OUT-RESTORED", buffer, StringComparison.Ordinal);
        Assert.Contains("ERROR-RESTORED", buffer, StringComparison.Ordinal);
    }
}
