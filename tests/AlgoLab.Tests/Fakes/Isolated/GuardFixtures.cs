using AlgoLab.Core.Contracts;

namespace AlgoLab.Tests.Fakes.Isolated;

// Namespace "AlgoLab.Tests.Fakes.Isolated", not "AlgoLab.Tests.Fakes": the shared Registry in
// ProblemRegistryTests scopes its scan to an exact match on "AlgoLab.Tests.Fakes", so these
// deliberately-broken fixtures never reach it. Each guard test instead calls
// ProblemRegistry.Build with exactly the types it needs.
public sealed record CollisionInput(int Value);

public sealed class CollisionProblemA : Problem<CollisionInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "collision-a",
        Title: "Коллизия A",
        Source: "https://example.com/collision-a",
        Difficulty: Difficulty.Easy,
        Tags: []);

    public override IEnumerable<TestCase<CollisionInput, int>> Cases =>
    [
        new(new CollisionInput(1), 1, "базовый"),
    ];
}

public sealed class CollisionProblemB : Problem<CollisionInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "collision-b",
        Title: "Коллизия B",
        Source: "https://example.com/collision-b",
        Difficulty: Difficulty.Easy,
        Tags: []);

    public override IEnumerable<TestCase<CollisionInput, int>> Cases =>
    [
        new(new CollisionInput(1), 1, "базовый"),
    ];
}

public sealed record BadConstructorInput(int Value);

/// <summary>Намеренно без публичного конструктора без параметров.</summary>
public sealed class BadConstructorProblem(int seed) : Problem<BadConstructorInput, int>
{
    private readonly int _seed = seed;

    public override ProblemInfo Info => throw new NotImplementedException($"seed={_seed}");

    public override IEnumerable<TestCase<BadConstructorInput, int>> Cases => throw new NotImplementedException();
}
