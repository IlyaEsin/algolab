using AlgoLab.Core.Json;

namespace AlgoLab.Core.Contracts;

/// <summary>Сравнение по содержимому через сериализацию: массивы и вложенные коллекции
/// сравниваются поэлементно без рефлексии по типам.
/// Ограничение: числа с плавающей точкой сравниваются точно — задачам с погрешностью
/// нужен свой <see cref="IOutputComparer{T}"/>.</summary>
public sealed class StructuralComparer<T> : IOutputComparer<T>
{
    public static StructuralComparer<T> Instance { get; } = new();

    private StructuralComparer()
    {
    }

    public bool Equals(T expected, T actual) =>
        string.Equals(AlgoLabJson.Describe(expected), AlgoLabJson.Describe(actual), StringComparison.Ordinal);
}
