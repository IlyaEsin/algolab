using System.Text;
using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;
using AlgoLab.Problems;
using AlgoLab.Runner;

// The console codepage the OS attaches at spawn time is not under our control (it differs
// between a real console and a redirected pipe), but the parent always deserializes the
// stream as UTF-8. Write through an explicitly-encoded, BOM-less writer so the bytes on
// stdout are deterministic regardless of what environment launches this process.
using var output = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

try
{
    var registry = ProblemRegistry.Build(ProblemsAssembly.Reference);
    var payload = RunnerCommand.Execute(args, registry);

    output.Write(JsonSerializer.Serialize(payload, AlgoLabJson.Options));
    output.Flush();
    return payload.Error is null ? 0 : 1;
}
catch (Exception exception)
{
    var payload = new RunnerPayload(
        Run: null,
        Series: null,
        Error: $"{exception.GetType().Name}: {exception.Message}");

    output.Write(JsonSerializer.Serialize(payload, AlgoLabJson.Options));
    output.Flush();
    return 2;
}
