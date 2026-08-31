using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Design.LruCache;

[Solution("Словарь плюс двусвязный список", Time = Complexity.ON, Space = Complexity.ON,
    Note = "Каждая операция — O(1); n здесь считает операции, поэтому рост линейный.")]
public sealed class LruCacheLinkedDictionary : ISolution<LruCacheInput, int?[]>
{
    public int?[] Solve(LruCacheInput input)
    {
        var order = new LinkedList<int>();
        var entries = new Dictionary<int, (int Value, LinkedListNode<int> Node)>(input.Capacity);
        var results = new int?[input.Operations.Count];

        for (var i = 0; i < input.Operations.Count; i++)
        {
            var operation = input.Operations[i];

            if (operation.Kind == LruOperationKind.Get)
            {
                if (entries.TryGetValue(operation.Key, out var entry))
                {
                    order.Remove(entry.Node);
                    order.AddFirst(entry.Node);
                    results[i] = entry.Value;
                }
                else
                {
                    results[i] = -1;
                }

                continue;
            }

            if (entries.TryGetValue(operation.Key, out var existing))
            {
                order.Remove(existing.Node);
                order.AddFirst(existing.Node);
                entries[operation.Key] = (operation.Value, existing.Node);
                continue;
            }

            if (entries.Count == input.Capacity)
            {
                var evicted = order.Last!;
                order.RemoveLast();
                entries.Remove(evicted.Value);
            }

            entries[operation.Key] = (operation.Value, order.AddFirst(operation.Key));
        }

        return results;
    }
}
