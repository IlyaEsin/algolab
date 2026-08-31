using System.Reflection;

namespace AlgoLab.Problems;

/// <summary>Маркер сборки задач — точка входа для рефлексии.</summary>
public static class ProblemsAssembly
{
    public static Assembly Reference => typeof(ProblemsAssembly).Assembly;
}
