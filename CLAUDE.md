# AlgoLab — Claude guidance

A local trainer for algorithm-practice problems. A problem ships with a
statement and reference solutions of different complexity; instead of trusting
a claimed Big-O, AlgoLab runs the solution on inputs of growing size and fits
the measured curve against the declared complexity class. The repository is
**public** — nothing committed here may contain credentials, tokens, or
private paths.

## Target framework: net9.0, never net10

`global.json` pins the SDK to `9.0.317` with `"rollForward": "latestFeature"`,
set this way from the project's first commit. The `Dockerfile` copies
`global.json` before `src/` for the same reason: `rollForward: latestFeature`
lets the pin float to whatever exact SDK patch the floating `sdk:9.0` Docker
tag resolves to, within the same feature band, instead of requiring an exact
match. Don't tighten or remove it.

Visual Studio 2022 (the user's IDE) cannot open .NET 10 projects. Every
`.csproj`/`Directory.Build.props` in this repo targets `net9.0` deliberately;
do not bump it.

**`dotnet add package` without an explicit version can resolve a
`net10.0`-only package build**, which then fails to restore against this
repo's `net9.0` target. This actually broke the build once (an unversioned
`Microsoft.AspNetCore.Mvc.Testing` add resolved `10.0.11`); a later package
(`Microsoft.AspNetCore.SpaProxy`) was pinned proactively to avoid a repeat.
Always pin an explicit version when adding a package:

```bash
dotnet add <project> package <Name> --version <exact-version>
```

## Repository map

- `src/AlgoLab.Core` — contracts and mechanics: `Problem<TInput,TOutput>`,
  `ISolution<TInput,TOutput>`, the `[Solution]` attribute, the reflection-based
  `ProblemRegistry`, the complexity-fitting measurer, and the embedded-resource
  `SourceStore`. No problems live here.
- `src/AlgoLab.Problems` — problems only, one folder per problem under a
  category folder (`Strings`, `Lists`, `HashTable`, `Heap`, `Tree`, `Graph`,
  `DynamicProgramming`, `Design`). Nothing here references `AlgoLab.Api` or
  `AlgoLab.Runner`.
- `src/AlgoLab.Runner` — executes a solution in a separate process, isolated
  from the API process, and serializes the result back over stdout as UTF-8.
- `src/AlgoLab.Api` — the ASP.NET HTTP API plus the Vite + React SPA under
  `src/AlgoLab.Api/web`, served through the SPA proxy in development.
- `tests/AlgoLab.Tests` — one test project covering all of the above,
  including the guard tests described below.

## Adding a problem

Use the `add-problem` skill — it walks through every file a new problem
needs. Two rules from that skill are worth repeating here. Neither is a
mistake this project has actually made — both are traps designed around from
the first problem onward, which is exactly why they never got the chance to
become one. (The `Space` and `Scaler` rules further down are the real
scars — they cost an actual fix.)

- **A problem's namespace must mirror its folder path.** `SourceStore` builds
  the embedded-resource name by concatenating the type's namespace with the
  file name (`{namespace}.{TypeName}.cs`, `{namespace}.README.md`). A
  namespace that doesn't match the folder makes the resource lookup miss,
  which breaks source and README display at runtime — a guard test
  (`RegistryIntegrityTests.Namespace_matches_the_slug_folder`) catches this.
- **Never name a category folder after a BCL type used inside it.** A folder
  named `Stack` produces namespace `AlgoLab.Problems.Stack`; a solution in
  that folder that writes `new Stack<char>()` then resolves `Stack` to the
  namespace, not to `System.Collections.Generic.Stack<T>`, and fails to
  compile. That's why the folders are `Strings` and `Lists`, not `Stack` or
  `Queue`.

Nothing needs registering anywhere else — `ProblemRegistry.Build` finds every
`Problem<,>` subclass in the `AlgoLab.Problems` assembly by reflection and
attaches every `ISolution<,>` whose (input, output) type pair matches.

### `Space` means heap allocations, not textbook auxiliary space — a real fix

`[Solution]`'s `Space` complexity is checked against heap bytes allocated
around a single `Solve` call (`GC.GetAllocatedBytesForCurrentThread` before
and after). **Stack usage — including recursion depth — is invisible to this
measurement.** This bit `ReverseLinkedListIterative`: it declared
`Space = Complexity.O1`, correct for its own three-pointer swap, but `Solve`
also marshals the input array into a linked list and back
(`ListNode.FromArray`/`ToArray`) before and after that swap, which allocates
O(n) on the heap regardless of which reversal algorithm runs — the measurer
scored it a false "расходится" (diverges) against a solution that was, in the
textbook sense, correct. Fixed by declaring `Space = Complexity.ON`.

The subtler point this surfaced: the thing that actually tells this solution
apart from its recursive sibling, `ReverseLinkedListRecursive`, is stack
space — the recursive version needs O(n) of call-stack depth, the iterative
one doesn't — and the measurer cannot see stack at all. Both solutions
honestly declare `Space = Complexity.ON` today, for the same reason (the
array/list marshalling), even though a textbook would tell them apart by
their recursion. Declaring textbook auxiliary space fails here precisely
because it answers a different question ("how much extra memory does the
algorithm conceptually need") than the one being measured ("how many heap
bytes does this `Solve` call allocate"). When declaring `Space`, ask the
second question.

### A `Scaler` must keep producing different input as `n` grows — a real fix

If a scaler clamps or saturates a value instead of continuing to scale it, the
input at large `n` stops changing, the growth curve goes flat, and any
declared complexity reads as a false divergence — not because the solution is
wrong, but because there is nothing left to measure. This hit
`ClimbingStairsMemoized`: its scaler clamped `Steps` to `Math.Clamp(n, 1,
90)`, so past n≈90 the input stopped changing and a genuinely `O(n)`-space
solution measured as a flat curve — a false "расходится" against the
declared class. The fix was to stop measuring this problem rather than patch
the clamp; `ClimbingStairs` no longer declares a `Scaler` at all. A guard
test
(`RegistryIntegrityTests.Scaler_input_still_grows_at_the_top_of_the_default_ladder`)
checks this at the top two rungs of the default measurement ladder for every
problem that does declare one.

### Problem statements are the author's own words

The repository is public; LeetCode's problem text is not ours to copy. Every
problem's `README.md` restates the problem in the author's own words with a
link to the original in `Info.Source`.

## Building and testing

Build and test only through the `build-test` skill — never call `dotnet
build`/`dotnet test`/`npm test` ad hoc outside it, so build and test output
stays consistent with what the skill's rule about reading failures expects.

Tests in `tests/AlgoLab.Tests` are serialized assembly-wide
(`[assembly: CollectionBehavior(DisableTestParallelization = true)]`) because
one test suite mutates the process-global `Console.Out`/`Console.Error`. The
suite is slower than its test count suggests — that's expected, not a
regression.

## Code style

Code should be self-documenting. Comment the *why*, never the *what*: a brief
flag for a non-obvious pitfall is fine, narrating what the next line of code
does is not.

## MCP: codebase-memory-mcp

This project uses `codebase-memory-mcp` for code-structure queries (where is
X, what calls Y, trace a call path). It is not registered by default — a
person working in this repo registers it once, at project scope, with:

```bash
claude mcp add codebase-memory-mcp --scope project -- "$LOCALAPPDATA/Programs/codebase-memory-mcp/codebase-memory-mcp.exe" mcp
```
