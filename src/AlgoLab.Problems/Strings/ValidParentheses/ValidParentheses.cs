using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Strings.ValidParentheses;

public sealed record ValidParenthesesInput(string Text);

public sealed class ValidParentheses : Problem<ValidParenthesesInput, bool>
{
    public override ProblemInfo Info => new(
        Slug: "valid-parentheses",
        Title: "Правильная скобочная последовательность",
        Source: "https://leetcode.com/problems/valid-parentheses/",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.String, Tag.Stack]);

    public override IEnumerable<TestCase<ValidParenthesesInput, bool>> Cases =>
    [
        new(new ValidParenthesesInput("()"), true, "простая пара"),
        new(new ValidParenthesesInput("()[]{}"), true, "три пары подряд"),
        new(new ValidParenthesesInput("(]"), false, "разные типы"),
        new(new ValidParenthesesInput("([)]"), false, "пересечение"),
        new(new ValidParenthesesInput("{[]}"), true, "вложенность"),
        new(new ValidParenthesesInput(""), true, "пустая строка"),
        new(new ValidParenthesesInput("("), false, "незакрытая"),
    ];

    public override IInputScaler<ValidParenthesesInput> Scaler => new ValidParenthesesScaler();
}

public sealed class ValidParenthesesScaler : IInputScaler<ValidParenthesesInput>
{
    public ValidParenthesesInput Create(int n, int seed) =>
        new(new string('(', n) + new string(')', n));
}
