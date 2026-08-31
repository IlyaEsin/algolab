using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Lists.ReverseLinkedList;

[Solution("Три указателя", Time = Complexity.ON, Space = Complexity.O1)]
public sealed class ReverseLinkedListIterative : ISolution<ReverseLinkedListInput, int[]>
{
    public int[] Solve(ReverseLinkedListInput input)
    {
        ListNode? previous = null;
        var current = ListNode.FromArray(input.Values);

        while (current is not null)
        {
            var next = current.Next;
            current.Next = previous;
            previous = current;
            current = next;
        }

        return ListNode.ToArray(previous);
    }
}
