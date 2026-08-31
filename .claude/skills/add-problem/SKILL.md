---
name: add-problem
description: Создаёт папку новой задачи по шаблону. Use when the user asks to add a new algorithm problem to AlgoLab.
allowed-tools: Read, Write, Edit, Bash, Glob
---

# Add a new problem to AlgoLab

Nothing needs registering anywhere: `ProblemRegistry.Build` finds every
`Problem<TInput,TOutput>` subclass in the `AlgoLab.Problems` assembly by
reflection, and attaches every `ISolution<TInput,TOutput>` whose (input,
output) type pair matches it. Creating the files below is the whole job.

Look at an existing problem (e.g.
`src/AlgoLab.Problems/Strings/ValidParentheses/`) for a concrete example of
every file described here.

## 1. Choose a category folder and slug

Pick an existing category folder under `src/AlgoLab.Problems/` (`Strings`,
`Lists`, `HashTable`, `Heap`, `Tree`, `Graph`, `DynamicProgramming`, `Design`)
or a new one if none fits. **Never name a category folder after a BCL type
used inside it** — a folder named `Stack` produces namespace
`AlgoLab.Problems.Stack`, and `new Stack<char>()` inside it then resolves to
the namespace instead of `System.Collections.Generic.Stack<T>` and fails to
compile. That's why the existing folders are `Strings` and `Lists`, not
`Stack` or `Queue`.

Pick a PascalCase problem name (e.g. `TwoSum`) and create:

```
src/AlgoLab.Problems/<Category>/<Name>/
```

**The namespace of every class in this folder must be exactly
`AlgoLab.Problems.<Category>.<Name>`** — `SourceStore` builds the
embedded-resource name by concatenating a type's namespace with its file
name, so a mismatched namespace breaks source and README lookup at runtime.
A guard test enforces this (`RegistryIntegrityTests.Namespace_matches_the_slug_folder`);
don't skip checking it by hand.

## 2. `<Name>.cs` — input record and problem class

```csharp
using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.<Category>.<Name>;

public sealed record <Name>Input(...);

public sealed class <Name> : Problem<<Name>Input, <TOutput>>
{
    public override ProblemInfo Info => new(
        Slug: "<kebab-case-slug>",
        Title: "<заголовок на русском>",
        Source: "<ссылка на оригинал>",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.<...>]);

    public override IEnumerable<TestCase<<Name>Input, <TOutput>>> Cases =>
    [
        new(new <Name>Input(...), <expected>, "обычный случай"),
        new(new <Name>Input(...), <expected>, "второй обычный случай"),
        new(new <Name>Input(...), <expected>, "граничный случай"),
    ];

    public override IInputScaler<<Name>Input> Scaler => new <Name>Scaler();
}

public sealed class <Name>Scaler : IInputScaler<<Name>Input>
{
    public <Name>Input Create(int n, int seed) => new(...);
}
```

Requirements:

- `Cases` needs **at least three** cases, and **at least one must be a
  boundary case** (empty input, single element, already-sorted, all-equal,
  etc — whatever is the edge for this problem).
- Add a `Scaler` only when the problem can be meaningfully measured (its
  runtime/allocations should actually change with `n`). If a scaler doesn't
  fit the problem, omit the `Scaler` override entirely — `Problem<,>.Scaler`
  defaults to `null` and the problem is simply not measured.
- The slug's kebab-case, minus hyphens, must equal the folder name
  (case-insensitive) — e.g. slug `two-sum` for folder `TwoSum`.

## 3. At least one reference solution, with both `Time` and `Space` declared

```csharp
using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.<Category>.<Name>;

[Solution("<название подхода>", Time = Complexity.ON, Space = Complexity.O1)]
public sealed class <Name><Approach> : ISolution<<Name>Input, <TOutput>>
{
    public <TOutput> Solve(<Name>Input input)
    {
        ...
    }
}
```

**Both `Time` and `Space` are mandatory** — a solution without them defaults
to `Complexity.Unknown`, and the guard test
`RegistryIntegrityTests.Reference_solutions_declare_their_complexity` fails
the build.

`Space` is checked against **heap bytes allocated around a single `Solve`
call**, not the textbook definition of auxiliary space. Stack usage —
including recursion depth — is invisible to that measurement. Declare the
heap allocation `Solve` actually performs, not what an algorithms textbook
would say about the algorithm's auxiliary space; getting this wrong on a
correct solution produces a false "расходится" (diverges) verdict, which
looks like a bug in the solution when the real bug is the declaration.

## 4. `<Name>Attempt.cs` — the unsolved stub

```csharp
using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.<Category>.<Name>;

[Solution("Моя попытка", Kind = SolutionKind.Attempt)]
public sealed class <Name>Attempt : ISolution<<Name>Input, <TOutput>>
{
    public <TOutput> Solve(<Name>Input input) => throw new NotImplementedException();
}
```

## 5. `README.md`

Sections, in the author's own words:

- The problem statement — **write it yourself, do not copy the original
  text**. The repository is public; LeetCode's wording is not ours to
  redistribute.
- A link to the original problem (same URL as `Info.Source`).
- A "Разбор" (analysis) section walking through the approach and its time /
  space complexity.

## 6. Check the namespace matches the folder

Before running tests, re-read every file created above and confirm the
namespace line reads `AlgoLab.Problems.<Category>.<Name>` exactly, matching
the folder path under `src/AlgoLab.Problems/`.

## 7. Run tests

Use the `build-test` skill (`dotnet test` covers the new problem
automatically — nothing to register). The guard tests in
`tests/AlgoLab.Tests/Problems/RegistryIntegrityTests.cs` will fail loudly if
the README is missing, there's no reference solution, `Time`/`Space` aren't
declared, the namespace doesn't match the folder, or the scaler stops
producing different input as `n` grows.
