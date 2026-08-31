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

public sealed record LoudInput(int Value);

public sealed class LoudProblem : Problem<LoudInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "fake-loud",
        Title: "Шумное решение",
        Source: "https://example.com/loud",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.Array]);

    public override IEnumerable<TestCase<LoudInput, int>> Cases =>
    [
        new(new LoudInput(2), 4, "базовый"),
    ];
}

/// <summary>Печатает в консоль — обычное дело для практикующегося, который отлаживается через
/// Console.WriteLine. Существует, чтобы проверить: RunnerProgram обязан спрятать Console.Out и
/// Console.Error решения за TextWriter.Null на время исполнения, иначе этот вывод попал бы в тот
/// же stdout, что и JSON-payload, и испортил бы его.</summary>
[Solution("Шумное решение", Time = Complexity.O1, Space = Complexity.O1)]
public sealed class LoudDoubles : ISolution<LoudInput, int>
{
    public int Solve(LoudInput input)
    {
        Console.WriteLine("{\"noise\": \"this would corrupt the payload if it reached stdout\"}");
        Console.Error.WriteLine("шум и в stderr");
        return input.Value * 2;
    }
}

public sealed record DeepRecursionInput(int Depth);

public sealed class DeepRecursionProblem : Problem<DeepRecursionInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "fake-deep-recursion",
        Title: "Глубокая рекурсия",
        Source: "https://example.com/deep-recursion",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.Array]);

    // Глубина подобрана измерением: на потоке с дефолтным стеком (~1 МБ) такая рекурсия
    // (без хвостового вызова — после Recurse ещё есть "+1") надёжно переполняется уже на
    // 25-30 тысячах кадров в Release, а 50 000 даёт запас с большим отрывом. На потоке
    // с большим стеком (см. RunnerProgram) она отрабатывает за миллисекунды.
    public override IEnumerable<TestCase<DeepRecursionInput, int>> Cases =>
    [
        new(new DeepRecursionInput(50_000), 50_000, "глубина, переполняющая стек по умолчанию"),
    ];
}

/// <summary>Рекурсия без хвостового вызова, глубина которой надёжно переполняет поток с
/// дефолтным стеком (~1 МБ) — существует, чтобы проверить, что раннер даёт решению
/// достаточно большой стек, а не падает необрабатываемым StackOverflowException, который
/// (в отличие от обычного исключения) убивает процесс целиком без единого байта на stdout.</summary>
[Solution("Рекурсия без хвостового вызова", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class DeepRecursion : ISolution<DeepRecursionInput, int>
{
    public int Solve(DeepRecursionInput input) => Recurse(input.Depth);

    private static int Recurse(int depth) => depth <= 0 ? 0 : 1 + Recurse(depth - 1);
}
