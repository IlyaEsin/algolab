using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;
using AlgoLab.Problems;

namespace AlgoLab.Runner;

/// <summary>Склеивает <see cref="RunnerCommand"/> с процессом: пишет payload в единственный
/// поток, который родитель разбирает как контракт. Решение исполняется с Console.Out/Error,
/// подменёнными на <see cref="TextWriter.Null"/> — иначе Console.WriteLine внутри чьей-то
/// попытки (обычное дело при отладке) попал бы в тот же stdout и испортил бы JSON.</summary>
public static class RunnerProgram
{
    public static int Run(string[] args, TextWriter output)
    {
        try
        {
            var registry = ProblemRegistry.Build(ProblemsAssembly.Reference);
            return Run(args, registry, output);
        }
        catch (Exception exception)
        {
            return WriteFailure(output, exception);
        }
    }

    public static int Run(string[] args, ProblemRegistry registry, TextWriter output)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;

        try
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);

            var payload = RunnerCommand.Execute(args, registry);
            Write(output, payload);
            return payload.Error is null ? 0 : 1;
        }
        catch (Exception exception)
        {
            return WriteFailure(output, exception);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private static int WriteFailure(TextWriter output, Exception exception)
    {
        var payload = new RunnerPayload(
            Run: null,
            Series: null,
            Error: $"{exception.GetType().Name}: {exception.Message}");

        Write(output, payload);
        return 2;
    }

    private static void Write(TextWriter output, RunnerPayload payload)
    {
        output.Write(JsonSerializer.Serialize(payload, AlgoLabJson.Options));
        output.Flush();
    }
}
