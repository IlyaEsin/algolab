---
name: build-test
description: Собирает решение и гоняет тесты .NET и фронта. Use when the user asks to build, test, or verify AlgoLab.
allowed-tools: Bash
---

# Build and test AlgoLab

Run in order, from the repository root:

```bash
dotnet build -c Debug
dotnet test --no-build -c Debug
cd src/AlgoLab.Api/web && npm test
```

Notes:

- `tests/AlgoLab.Tests` is serialized assembly-wide (one suite mutates the
  process-global `Console.Out`/`Console.Error`), so `dotnet test` is slower
  than its test count suggests. That is expected.
- `npm test` runs the frontend suite once (`vitest run`) and exits; it does
  not watch.

## If a test fails

**Read the whole output before rerunning anything.** A failure that looks
flaky at a glance is usually a real defect with a first look that didn't add
up — rerunning it is how real defects get dismissed as noise. Only rerun
after you have read the full failure output (assertion, stack trace, and any
surrounding test output) and formed a hypothesis for why it failed.
