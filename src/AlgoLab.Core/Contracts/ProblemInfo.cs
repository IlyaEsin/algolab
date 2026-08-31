namespace AlgoLab.Core.Contracts;

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
}

public enum Tag
{
    Array,
    String,
    HashTable,
    TwoPointers,
    SlidingWindow,
    BinarySearch,
    LinkedList,
    Tree,
    Graph,
    Matrix,
    DynamicProgramming,
    Heap,
    Stack,
    Sorting,
    Greedy,
    Design,
}

public sealed record ProblemInfo(
    string Slug,
    string Title,
    string Source,
    Difficulty Difficulty,
    IReadOnlyList<Tag> Tags);
