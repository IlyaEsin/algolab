using AlgoLab.Api.Execution;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlgoLab.Tests.Api;

// Guards the fix for finding 1: RunnerHost.ParsePayload is a pure transform from the runner's
// captured stdout text to a RunnerPayload, with no process involved — so it is tested directly
// with strings, no spawn or hooks required.
public sealed class RunnerHostTests
{
    [Fact]
    public void Malformed_json_is_reported_without_throwing()
    {
        var payload = RunnerHost.ParsePayload("{ not json", NullLogger.Instance);

        Assert.Equal("Не удалось разобрать ответ исполнителя.", payload.Error);
        Assert.Null(payload.Run);
        Assert.Null(payload.Series);
    }

    [Fact]
    public void Literal_json_null_is_reported_as_an_unparseable_response()
    {
        var payload = RunnerHost.ParsePayload("null", NullLogger.Instance);

        Assert.Equal("Не удалось разобрать ответ исполнителя.", payload.Error);
    }

    [Fact]
    public void Empty_output_is_reported_distinctly_from_malformed_output()
    {
        var payload = RunnerHost.ParsePayload(string.Empty, NullLogger.Instance);

        Assert.Equal("Исполнитель ничего не вернул.", payload.Error);
    }
}
