using AlgoLab.Core.Contracts;

namespace AlgoLab.Fixtures.BadConstructor;

// Своя сборка по той же причине, что и AlgoLab.Fixtures.ShapeCollision: ProblemRegistry.Build
// сканирует сборку целиком, поэтому тип без публичного конструктора без параметров нельзя
// положить рядом с рабочими фейками — иначе Build падает для каждого теста, использующего
// общий Registry, а не только для теста, который проверяет это сообщение об ошибке.
public sealed record BadConstructorInput(int Value);

/// <summary>Намеренно без публичного конструктора без параметров.</summary>
public sealed class BadConstructorProblem(int seed) : Problem<BadConstructorInput, int>
{
    private readonly int _seed = seed;

    public override ProblemInfo Info => throw new NotImplementedException($"seed={_seed}");

    public override IEnumerable<TestCase<BadConstructorInput, int>> Cases => throw new NotImplementedException();
}
