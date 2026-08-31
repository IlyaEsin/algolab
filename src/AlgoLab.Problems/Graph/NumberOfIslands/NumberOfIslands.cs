using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Graph.NumberOfIslands;

public sealed record NumberOfIslandsInput(string[] Grid);

public sealed class NumberOfIslands : Problem<NumberOfIslandsInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "number-of-islands",
        Title: "Количество островов",
        Source: "https://leetcode.com/problems/number-of-islands/",
        Difficulty: Difficulty.Medium,
        Tags: [Tag.Graph, Tag.Matrix]);

    public override IEnumerable<TestCase<NumberOfIslandsInput, int>> Cases =>
    [
        new(new NumberOfIslandsInput(["11110", "11010", "11000", "00000"]), 1, "один остров"),
        new(new NumberOfIslandsInput(["11000", "11000", "00100", "00011"]), 3, "три острова"),
        new(new NumberOfIslandsInput(["000", "000"]), 0, "только вода"),
        new(new NumberOfIslandsInput(["1"]), 1, "одна клетка суши"),
    ];

    public override IInputScaler<NumberOfIslandsInput> Scaler => new NumberOfIslandsScaler();
}

/// <summary>Сторона сетки растёт как sqrt(n), чтобы n соответствовало числу клеток —
/// иначе кривая показывала бы сложность по стороне, а не по размеру входа.</summary>
public sealed class NumberOfIslandsScaler : IInputScaler<NumberOfIslandsInput>
{
    public NumberOfIslandsInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var side = Math.Max(1, (int)Math.Sqrt(n));
        var grid = new string[side];
        for (var row = 0; row < side; row++)
        {
            var line = new char[side];
            for (var column = 0; column < side; column++)
            {
                line[column] = random.Next(0, 2) == 0 ? '0' : '1';
            }

            grid[row] = new string(line);
        }

        return new NumberOfIslandsInput(grid);
    }
}
