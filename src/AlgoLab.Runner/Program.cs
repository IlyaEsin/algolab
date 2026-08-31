using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;
using AlgoLab.Problems;
using AlgoLab.Runner;

try
{
    var registry = ProblemRegistry.Build(ProblemsAssembly.Reference);
    var payload = RunnerCommand.Execute(args, registry);

    Console.Out.Write(JsonSerializer.Serialize(payload, AlgoLabJson.Options));
    return payload.Error is null ? 0 : 1;
}
catch (Exception exception)
{
    var payload = new RunnerPayload(
        Run: null,
        Series: null,
        Error: $"{exception.GetType().Name}: {exception.Message}");

    Console.Out.Write(JsonSerializer.Serialize(payload, AlgoLabJson.Options));
    return 2;
}
