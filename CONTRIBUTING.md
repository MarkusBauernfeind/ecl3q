# Contributing to ECL₃^Q

Thank you for your interest in ECL₃^Q.

## Before Contributing

This repository is the reference implementation of a formal logical system
described in papers under preparation. The formal results (truth tables,
frame-definability, σ-conflict taxonomy, etc.) are fixed by the theory —
contributions that alter these would require coordinating with the paper author.

Good contributions:
- Bug fixes in existing implementations
- Additional tests exposing incorrect behaviour
- Performance improvements that do not change semantics
- Documentation improvements and corrections
- New examples illustrating ECL₃^Q in application domains

## Reporting Issues

Please open a GitHub issue with:
1. A minimal reproducible example (formula, model, or test case)
2. Expected vs actual behaviour
3. Which fragment is affected (propositional, modal, Obs, σ, multi-agent)

If you believe you have found an error in the formal results themselves
(not the implementation), please contact the author directly.

## Pull Requests

- One logical change per PR
- All existing tests must pass
- New behaviour must be accompanied by tests
- Follow the existing XML documentation style for public APIs
- Invariants in `ECL3Q.Core/Algebra/TruthValue.cs` (section "Critical Invariants")
  must not be violated under any circumstances

## Formal Invariants (non-negotiable)

These reflect proven mathematical results and must not be changed:

| Invariant | Location |
|-----------|----------|
| U is ontological, not epistemic | `TruthValue.cs`, all comments |
| Obs is not F₃-linear (Theorem A4) | `ObsOperator.cs` — no matrix representation |
| OC is falsified; only OC-weak holds | `ObsOperator.cs`, `AlgebraTests.cs` |
| SC_PC is a theorem under Option B, not an axiom | `Frame.cs`, `Model.cs` |
| RF1 and RF3 are frame-definable; RF2 is not | `Frame.cs`, `FrameAndTaxonomyTests.cs` |

## Code Style

- C# 12, .NET 8
- `sealed` classes where inheritance is not intended
- `readonly record struct` for value types used as dictionary keys
- XML `<summary>` on all public members
- Explicit `IReadOnlyList<>` over `IEnumerable<>` where multiple iteration is possible

## Open Problems

The following are known open problems in the theory, not implementation bugs:

- **F4**: Linear representation of Obs in a larger algebraic structure
- **Open C**: `U:Obs(φ)@w` from Source 2 (classical φ, disagreeing R_obs-successors)
- **Completeness theorem**: Full ECL₃^Q completeness for SC_PC + AO1_eigen frame class

Contributions addressing these are welcome but require theoretical justification.
