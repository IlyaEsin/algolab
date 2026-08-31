using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Design.LruCache;

public enum LruOperationKind
{
    Get,
    Put,
}

public sealed record LruOperation(LruOperationKind Kind, int Key, int Value);

public sealed record LruCacheInput(int Capacity, IReadOnlyList<LruOperation> Operations);

/// <summary>Design-задача: вход — последовательность операций, выход — результаты Get
/// (для Put результата нет, поэтому в выходе стоит null).</summary>
public sealed class LruCache : Problem<LruCacheInput, int?[]>
{
    public override ProblemInfo Info => new(
        Slug: "lru-cache",
        Title: "LRU-кеш",
        Source: "https://leetcode.com/problems/lru-cache/",
        Difficulty: Difficulty.Medium,
        Tags: [Tag.Design, Tag.HashTable, Tag.LinkedList]);

    public override IEnumerable<TestCase<LruCacheInput, int?[]>> Cases =>
    [
        new(
            new LruCacheInput(2,
            [
                new(LruOperationKind.Put, 1, 1),
                new(LruOperationKind.Put, 2, 2),
                new(LruOperationKind.Get, 1, 0),
                new(LruOperationKind.Put, 3, 3),
                new(LruOperationKind.Get, 2, 0),
                new(LruOperationKind.Get, 3, 0),
            ]),
            [null, null, 1, null, -1, 3],
            "вытеснение самого давнего"),
        new(
            new LruCacheInput(1,
            [
                new(LruOperationKind.Put, 1, 1),
                new(LruOperationKind.Put, 2, 2),
                new(LruOperationKind.Get, 1, 0),
            ]),
            [null, null, -1],
            "ёмкость один"),
        new(
            new LruCacheInput(2,
            [
                new(LruOperationKind.Put, 1, 1),
                new(LruOperationKind.Put, 1, 5),
                new(LruOperationKind.Get, 1, 0),
            ]),
            [null, null, 5],
            "перезапись ключа"),
    ];

    public override IInputScaler<LruCacheInput> Scaler => new LruCacheScaler();
}

public sealed class LruCacheScaler : IInputScaler<LruCacheInput>
{
    public LruCacheInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var operations = new List<LruOperation>(n);
        for (var i = 0; i < n; i++)
        {
            var key = random.Next(0, 512);
            operations.Add(random.Next(0, 2) == 0
                ? new LruOperation(LruOperationKind.Get, key, 0)
                : new LruOperation(LruOperationKind.Put, key, random.Next()));
        }

        return new LruCacheInput(Capacity: 128, operations);
    }
}
