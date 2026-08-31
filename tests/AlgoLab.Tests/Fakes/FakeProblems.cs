using AlgoLab.Core.Contracts;

namespace AlgoLab.Tests.Fakes;

public sealed record SumInput(int[] Values);

public sealed class SumProblem : Problem<SumInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "fake-sum",
        Title: "Сумма массива",
        Source: "https://example.com/sum",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.Array]);

    public override IEnumerable<TestCase<SumInput, int>> Cases =>
    [
        new(new SumInput([1, 2, 3]), 6, "базовый"),
        new(new SumInput([]), 0, "пустой массив"),
    ];

    public override IInputScaler<SumInput> Scaler => new SumScaler();
}

public sealed class SumScaler : IInputScaler<SumInput>
{
    public SumInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var values = new int[n];
        for (var i = 0; i < n; i++)
        {
            values[i] = random.Next(0, 100);
        }

        return new SumInput(values);
    }
}

[Solution("Цикл", Time = Complexity.ON, Space = Complexity.O1)]
public sealed class SumLoop : ISolution<SumInput, int>
{
    public int Solve(SumInput input)
    {
        var total = 0;
        foreach (var value in input.Values)
        {
            total += value;
        }

        return total;
    }
}

[Solution("Моя попытка", Kind = SolutionKind.Attempt)]
public sealed class SumAttempt : ISolution<SumInput, int>
{
    public int Solve(SumInput input) => throw new NotImplementedException();
}

public sealed record NoiseInput(string Text);

public sealed class NoiseProblem : Problem<NoiseInput, string>
{
    public override ProblemInfo Info => new(
        Slug: "fake-noise",
        Title: "Разворот строки",
        Source: "https://example.com/noise",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.String]);

    public override IEnumerable<TestCase<NoiseInput, string>> Cases =>
    [
        new(new NoiseInput("abc"), "cba", "базовый"),
    ];
}

[Solution("Реверс", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class NoiseReverse : ISolution<NoiseInput, string>
{
    public string Solve(NoiseInput input) => new(input.Text.Reverse().ToArray());
}

public sealed record WrongInput(int Value);

public sealed class WrongProblem : Problem<WrongInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "fake-wrong",
        Title: "Удвоение",
        Source: "https://example.com/wrong",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.Array]);

    public override IEnumerable<TestCase<WrongInput, int>> Cases =>
    [
        new(new WrongInput(2), 4, "двойка"),
    ];
}

[Solution("Всегда ноль", Time = Complexity.O1, Space = Complexity.O1)]
public sealed class WrongAlwaysZero : ISolution<WrongInput, int>
{
    public int Solve(WrongInput input) => 0;
}

[Solution("Бросает", Time = Complexity.O1, Space = Complexity.O1)]
public sealed class WrongThrows : ISolution<WrongInput, int>
{
    public int Solve(WrongInput input) => throw new InvalidOperationException("сломано");
}

public sealed record SlowInput(int[] Values);

public sealed class SlowProblem : Problem<SlowInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "fake-slow",
        Title: "Квадратичная сумма",
        Source: "https://example.com/slow",
        Difficulty: Difficulty.Medium,
        Tags: [Tag.Array]);

    public override IEnumerable<TestCase<SlowInput, int>> Cases =>
    [
        // Пары (1,1),(1,2),(2,1),(2,2) дают 2+3+3+4.
        new(new SlowInput([1, 2]), 12, "все пары"),
    ];

    public override IInputScaler<SlowInput> Scaler => new SlowScaler();
}

public sealed class SlowScaler : IInputScaler<SlowInput>
{
    public SlowInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var values = new int[n];
        for (var i = 0; i < n; i++)
        {
            values[i] = random.Next(0, 10);
        }

        return new SlowInput(values);
    }
}

[Solution("Все пары", Time = Complexity.ON2, Space = Complexity.ON)]
public sealed class SlowQuadratic : ISolution<SlowInput, int>
{
    public int Solve(SlowInput input)
    {
        var copy = input.Values.ToArray();
        var total = 0;
        for (var i = 0; i < copy.Length; i++)
        {
            for (var j = 0; j < copy.Length; j++)
            {
                total += copy[i] + copy[j];
            }
        }

        return total;
    }
}
