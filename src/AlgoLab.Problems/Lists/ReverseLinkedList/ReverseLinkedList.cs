using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Lists.ReverseLinkedList;

public sealed record ReverseLinkedListInput(int[] Values);

public sealed class ListNode(int value)
{
    public int Value { get; } = value;

    public ListNode? Next { get; set; }

    public static ListNode? FromArray(int[] values)
    {
        ListNode? head = null;
        for (var i = values.Length - 1; i >= 0; i--)
        {
            head = new ListNode(values[i]) { Next = head };
        }

        return head;
    }

    public static int[] ToArray(ListNode? head)
    {
        var result = new List<int>();
        for (var node = head; node is not null; node = node.Next)
        {
            result.Add(node.Value);
        }

        return result.ToArray();
    }
}

public sealed class ReverseLinkedList : Problem<ReverseLinkedListInput, int[]>
{
    public override ProblemInfo Info => new(
        Slug: "reverse-linked-list",
        Title: "Разворот связного списка",
        Source: "https://leetcode.com/problems/reverse-linked-list/",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.LinkedList]);

    public override IEnumerable<TestCase<ReverseLinkedListInput, int[]>> Cases =>
    [
        new(new ReverseLinkedListInput([1, 2, 3, 4, 5]), [5, 4, 3, 2, 1], "пять узлов"),
        new(new ReverseLinkedListInput([1, 2]), [2, 1], "два узла"),
        new(new ReverseLinkedListInput([]), [], "пустой список"),
    ];

    public override IInputScaler<ReverseLinkedListInput> Scaler => new ReverseLinkedListScaler();
}

public sealed class ReverseLinkedListScaler : IInputScaler<ReverseLinkedListInput>
{
    public ReverseLinkedListInput Create(int n, int seed) =>
        new(Enumerable.Range(0, n).ToArray());
}
