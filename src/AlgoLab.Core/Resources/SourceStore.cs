using System.Reflection;
using AlgoLab.Core.Registry;

namespace AlgoLab.Core.Resources;

/// <summary>Читает исходники и условия задач из embedded-ресурсов сборки задач.
/// Имя ресурса складывается из namespace и имени файла, поэтому namespace обязан
/// совпадать с путём папки.</summary>
public static class SourceStore
{
    public static string ReadSource(Type type)
    {
        var assembly = type.Assembly;
        var name = $"{type.Namespace}.{type.Name}.cs";
        return Read(assembly, name);
    }

    public static string ReadReadme(ProblemDescriptor problem)
    {
        var assembly = problem.ProblemType.Assembly;
        var name = $"{problem.Namespace}.README.md";
        return Read(assembly, name);
    }

    private static string Read(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Ресурс '{resourceName}' не найден. Проверь, что namespace совпадает с папкой "
                + $"и что файл попадает в <EmbeddedResource>. Доступны: "
                + string.Join(", ", assembly.GetManifestResourceNames()));

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
