namespace AlgoLab.Core.Contracts;

public sealed record TestCase<TInput, TOutput>(TInput Input, TOutput Expected, string Name);
