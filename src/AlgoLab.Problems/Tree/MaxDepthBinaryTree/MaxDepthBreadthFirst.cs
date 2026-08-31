using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Tree.MaxDepthBinaryTree;

[Solution("Обход в ширину", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class MaxDepthBreadthFirst : ISolution<MaxDepthBinaryTreeInput, int>
{
    public int Solve(MaxDepthBinaryTreeInput input)
    {
        var root = TreeNode.FromLevelOrder(input.LevelOrder);
        if (root is null)
        {
            return 0;
        }

        var queue = new Queue<TreeNode>();
        queue.Enqueue(root);
        var depth = 0;

        while (queue.Count > 0)
        {
            depth++;
            for (var i = queue.Count; i > 0; i--)
            {
                var node = queue.Dequeue();
                if (node.Left is not null)
                {
                    queue.Enqueue(node.Left);
                }

                if (node.Right is not null)
                {
                    queue.Enqueue(node.Right);
                }
            }
        }

        return depth;
    }
}
