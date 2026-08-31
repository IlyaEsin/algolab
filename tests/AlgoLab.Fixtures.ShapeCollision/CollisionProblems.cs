using AlgoLab.Core.Contracts;

namespace AlgoLab.Fixtures.ShapeCollision;

// Отдельная сборка, а не файл в AlgoLab.Tests: ProblemRegistry.Build сканирует всю сборку,
// а не отдельные файлы, и общий статический Registry в ProblemRegistryTests пересобирается
// на каждое обращение через Assembly.GetExecutingAssembly(). Помести эту пару типов в
// AlgoLab.Tests (даже как вложенный private-класс) — и коллизия будет брошена для каждого
// теста в файле, а не только для теста, который её проверяет.
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
