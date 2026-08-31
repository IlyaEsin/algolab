namespace AlgoLab.Tests;

public sealed class ToolchainTests
{
    [Fact]
    public void Tests_run_on_dotnet_9()
    {
        Assert.Equal(9, Environment.Version.Major);
    }
}
