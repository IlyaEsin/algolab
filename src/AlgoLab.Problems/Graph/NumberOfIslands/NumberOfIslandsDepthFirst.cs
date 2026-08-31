using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Graph.NumberOfIslands;

[Solution("Обход в глубину со стеком", Time = Complexity.ON, Space = Complexity.ON,
    Note = "n — число клеток сетки.")]
public sealed class NumberOfIslandsDepthFirst : ISolution<NumberOfIslandsInput, int>
{
    public int Solve(NumberOfIslandsInput input)
    {
        var rows = input.Grid.Length;
        if (rows == 0)
        {
            return 0;
        }

        var columns = input.Grid[0].Length;
        var seen = new bool[rows, columns];
        var islands = 0;
        var stack = new Stack<(int Row, int Column)>();

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                if (input.Grid[row][column] != '1' || seen[row, column])
                {
                    continue;
                }

                islands++;
                stack.Push((row, column));
                seen[row, column] = true;

                while (stack.Count > 0)
                {
                    var (r, c) = stack.Pop();
                    foreach (var (dr, dc) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        var nr = r + dr;
                        var nc = c + dc;
                        if (nr < 0 || nr >= rows || nc < 0 || nc >= columns)
                        {
                            continue;
                        }

                        if (input.Grid[nr][nc] == '1' && !seen[nr, nc])
                        {
                            seen[nr, nc] = true;
                            stack.Push((nr, nc));
                        }
                    }
                }
            }
        }

        return islands;
    }
}
