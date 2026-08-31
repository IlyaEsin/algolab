using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Running;

namespace AlgoLab.Api.Execution;

/// <summary>Исполняет код задач в отдельном процессе. Поток с бесконечным циклом
/// в .NET снаружи не остановить, поэтому единственный надёжный способ ограничить
/// попытку по времени — убить процесс целиком.</summary>
public sealed class RunnerHost(ILogger<RunnerHost> logger)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    // Исполнитель пишет в stdout UTF-8 без BOM — все сообщения для пользователя на русском.
    // Родитель должен читать тем же кодированием, иначе текст придёт побитым, а не упадёт с ошибкой.
    private static readonly Encoding OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static string ExecutablePath => Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "AlgoLab.Runner.exe" : "AlgoLab.Runner");

    public Task<RunnerPayload> RunAsync(string slug, string solutionId, CancellationToken cancellationToken) =>
        ExecuteAsync(["cases", "--slug", slug, "--solution", solutionId], cancellationToken);

    public Task<RunnerPayload> MeasureAsync(
        string slug,
        IReadOnlyList<string> solutionIds,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string> { "measure", "--slug", slug };
        foreach (var id in solutionIds)
        {
            arguments.Add("--solution");
            arguments.Add(id);
        }

        return ExecuteAsync(arguments, cancellationToken);
    }

    private async Task<RunnerPayload> ExecuteAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo(ExecutablePath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = OutputEncoding,
            StandardErrorEncoding = OutputEncoding,
            UseShellExecute = false,
        };

        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        using var process = Process.Start(info)
            ?? throw new InvalidOperationException($"Не удалось запустить {ExecutablePath}.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        var stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            Kill(process);

            // Killing the process eventually unblocks these reads, but they are still pending
            // right here — leaving them unobserved risks an unobserved-task-exception once the
            // pipes close mid-read and the task objects get collected. Observe them before
            // returning; their content is irrelevant once the run has been declared timed out.
            await ObserveAsync(stdout);
            await ObserveAsync(stderr);

            return new RunnerPayload(
                Run: null,
                Series: null,
                Error: $"Исполнение прервано по таймауту в {Timeout.TotalSeconds:N0} с — похоже на бесконечный цикл.");
        }

        var output = await stdout;
        var errors = await stderr;

        if (!string.IsNullOrWhiteSpace(errors))
        {
            logger.LogWarning("Runner stderr: {Errors}", errors);
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            return new RunnerPayload(Run: null, Series: null, Error: "Исполнитель ничего не вернул.");
        }

        try
        {
            return JsonSerializer.Deserialize<RunnerPayload>(output, AlgoLabJson.Options)
                ?? new RunnerPayload(Run: null, Series: null, Error: "Не удалось разобрать ответ исполнителя.");
        }
        catch (JsonException exception)
        {
            // The raw output stays in the log, not in the payload: it can contain whatever a
            // stray print inside someone's attempt wrote, which is not fit for the browser.
            logger.LogWarning(exception, "Не удалось разобрать ответ исполнителя. Сырой вывод: {Output}", output);
            return new RunnerPayload(Run: null, Series: null, Error: "Не удалось разобрать ответ исполнителя.");
        }
    }

    private static async Task ObserveAsync(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            // Процесс уже убит по таймауту — что бы ни случилось при чтении из его труб,
            // на решение о таймауте это уже не влияет.
        }
    }

    private void Kill(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Не удалось убить процесс исполнителя.");
        }
    }
}
