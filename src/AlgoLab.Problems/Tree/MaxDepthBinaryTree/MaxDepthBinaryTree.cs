using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Tree.MaxDepthBinaryTree;

/// <summary>Дерево задаётся обходом в ширину, где <c>null</c> — отсутствующий узел.</summary>
public sealed record MaxDepthBinaryTreeInput(int?[] LevelOrder);

public sealed class TreeNode(int value)
{
    public int Value { get; } = value;

    public TreeNode? Left { get; set; }

    public TreeNode? Right { get; set; }

    public static TreeNode? FromLevelOrder(int?[] levelOrder)
    {
        if (levelOrder.Length == 0 || levelOrder[0] is null)
        {
            return null;
        }

        var root = new TreeNode(levelOrder[0]!.Value);
        var queue = new Queue<TreeNode>();
        queue.Enqueue(root);
        var index = 1;

        while (queue.Count > 0 && index < levelOrder.Length)
        {
            var node = queue.Dequeue();

            if (index < levelOrder.Length && levelOrder[index] is { } left)
            {
                node.Left = new TreeNode(left);
                queue.Enqueue(node.Left);
            }

            index++;

            if (index < levelOrder.Length && levelOrder[index] is { } right)
            {
                node.Right = new TreeNode(right);
                queue.Enqueue(node.Right);
            }

            index++;
        }

        return root;
    }
}

public sealed class MaxDepthBinaryTree : Problem<MaxDepthBinaryTreeInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "max-depth-binary-tree",
        Title: "Глубина двоичного дерева",
        Source: "https://leetcode.com/problems/maximum-depth-of-binary-tree/",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.Tree, Tag.Graph]);

    public override IEnumerable<TestCase<MaxDepthBinaryTreeInput, int>> Cases =>
    [
        new(new MaxDepthBinaryTreeInput([3, 9, 20, null, null, 15, 7]), 3, "классическое дерево"),
        new(new MaxDepthBinaryTreeInput([1, null, 2]), 2, "вырожденное вправо"),
        new(new MaxDepthBinaryTreeInput([]), 0, "пустое дерево"),
        new(new MaxDepthBinaryTreeInput([1]), 1, "один узел"),
    ];

    public override IInputScaler<MaxDepthBinaryTreeInput> Scaler => new MaxDepthBinaryTreeScaler();
}

public sealed class MaxDepthBinaryTreeScaler : IInputScaler<MaxDepthBinaryTreeInput>
{
    public MaxDepthBinaryTreeInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var values = new int?[n];
        for (var i = 0; i < n; i++)
        {
            values[i] = random.Next(0, 1000);
        }

        return new MaxDepthBinaryTreeInput(values);
    }
}
