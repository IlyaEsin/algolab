using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Tree.MaxDepthBinaryTree;

[Solution("Обход в глубину", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class MaxDepthRecursive : ISolution<MaxDepthBinaryTreeInput, int>
{
    public int Solve(MaxDepthBinaryTreeInput input) =>
        Depth(TreeNode.FromLevelOrder(input.LevelOrder));

    private static int Depth(TreeNode? node) =>
        node is null ? 0 : 1 + Math.Max(Depth(node.Left), Depth(node.Right));
}
