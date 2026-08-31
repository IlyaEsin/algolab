using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Lists.ReverseLinkedList;

[Solution("Рекурсия", Time = Complexity.ON, Space = Complexity.ON,
    Note = "Глубина рекурсии равна длине списка — на длинном входе переполнит стек.")]
public sealed class ReverseLinkedListRecursive : ISolution<ReverseLinkedListInput, int[]>
{
    public int[] Solve(ReverseLinkedListInput input) =>
        ListNode.ToArray(Reverse(ListNode.FromArray(input.Values), previous: null));

    private static ListNode? Reverse(ListNode? current, ListNode? previous)
    {
        if (current is null)
        {
            return previous;
        }

        var next = current.Next;
        current.Next = previous;
        return Reverse(next, current);
    }
}
