using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Strings.ValidParentheses;

[Solution("Стек", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class ValidParenthesesStack : ISolution<ValidParenthesesInput, bool>
{
    private static readonly Dictionary<char, char> Pairs = new() { [')'] = '(', [']'] = '[', ['}'] = '{' };

    public bool Solve(ValidParenthesesInput input)
    {
        var open = new Stack<char>();
        foreach (var symbol in input.Text)
        {
            if (Pairs.TryGetValue(symbol, out var expected))
            {
                if (open.Count == 0 || open.Pop() != expected)
                {
                    return false;
                }
            }
            else
            {
                open.Push(symbol);
            }
        }

        return open.Count == 0;
    }
}
