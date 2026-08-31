using System.Diagnostics;
using System.Text.Json;
using AlgoLab.Core.Json;
using AlgoLab.Core.Running;

namespace AlgoLab.Tests.Runner;

public sealed class RunnerProcessTests
{
    private static string ExecutablePath => Path.Combine(
        AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "AlgoLab.Runner.exe" : "AlgoLab.Runner");

    [Fact]
    public void Runner_executable_is_copied_next_to_the_tests()
    {
        Assert.True(File.Exists(ExecutablePath), $"Не найден {ExecutablePath}");
    }

    [Fact]
    public void Runner_prints_json_and_exits_with_zero()
    {
        var info = new ProcessStartInfo(ExecutablePath)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add("cases");
        info.ArgumentList.Add("--slug");
        info.ArgumentList.Add("two-sum");
        info.ArgumentList.Add("--solution");
        info.ArgumentList.Add("TwoSumHashMap");

        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);

        var payload = JsonSerializer.Deserialize<RunnerPayload>(stdout, AlgoLabJson.Options)!;
        Assert.Equal(RunStatus.Passed, payload.Run!.Status);
    }
}
