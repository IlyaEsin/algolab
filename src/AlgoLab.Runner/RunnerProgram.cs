using System.Runtime.ExceptionServices;
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
    /// <summary>Стек потока по умолчанию (около 1 МБ) слишком мал для решений, которые
    /// рекурсируют по природе задачи (обходы деревьев, списков, разбиение пополам) — на
    /// большом входе они переполняют его. StackOverflowException в .NET нельзя поймать,
    /// поэтому процесс просто падает без единого байта на stdout. Вместо того чтобы ужимать
    /// задачи под тесный стек, решение исполняется на выделенном потоке с большим — так
    /// рекурсия остаётся естественным способом решения, а не тем, что ломает исполнителя.</summary>
    private const int SolutionStackSizeBytes = 64 * 1024 * 1024;

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

            var payload = RunOnDedicatedStack(() => RunnerCommand.Execute(args, registry));
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

    /// <summary>Выполняет работу на отдельном потоке с большим стеком и ждёт её завершения.
    /// Console.Out/Error — статические свойства, общие на весь процесс, а не для потока, поэтому
    /// подмена в вызывающем потоке действует и здесь без какой-либо дополнительной настройки.
    /// Исключение из рабочего потока перевыбрасывается в вызывающем через
    /// <see cref="ExceptionDispatchInfo"/> — тип, сообщение и стек не меняются, так что
    /// обработка ошибок выше остаётся такой же, как если бы работа выполнялась синхронно.</summary>
    private static T RunOnDedicatedStack<T>(Func<T> work)
    {
        T? result = default;
        ExceptionDispatchInfo? failure = null;

        var worker = new Thread(() =>
        {
            try
            {
                result = work();
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
            }
        }, SolutionStackSizeBytes);

        worker.Start();
        worker.Join();

        failure?.Throw();
        return result!;
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
