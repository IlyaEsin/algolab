using System.Reflection;
using AlgoLab.Core.Registry;

namespace AlgoLab.Tests.Fakes;

public static class FakeRegistry
{
    // Точное совпадение по namespace, а не StartsWith: так изолированные фикстуры для
    // гвардов (namespace "AlgoLab.Tests.Fakes.Isolated") могут жить рядом, не попадая
    // в общий реестр, а любые новые фейки в "AlgoLab.Tests.Fakes" подхватываются сами,
    // без правки этого фильтра.
    public static ProblemRegistry Instance => ProblemRegistry.Build(
        Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "AlgoLab.Tests.Fakes"));
}
