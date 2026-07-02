# ECL₃^Q — Epistemic Causal Logic, Three-Valued, Quantum Extension

A formal logical system implementing **ECL₃^Q**: a three-valued epistemic causal logic
with quantum-inspired observability semantics.

## What is ECL₃^Q?

ECL₃^Q extends Strong Kleene three-valued logic (K₃) with:
- An **observability operator** `Obs(A)` modeling what can be witnessed in a given world
- A **σ-collapse operator** modeling the transition from ontological superposition to eigenstate
- **Modal operators** `□` and `◇` over an observation relation R_obs
- A **dynamic operator** `[do τ]` for action-induced collapse (τ-Framework)
- A **multi-agent architecture** with individual observation relations and σ-carrier authority

The system properly contains both classical logic and K₃ as sub-logics, and is
non-embeddable in Birkhoff–von Neumann quantum logic in either direction.

## The Critical Semantic Distinction

The third truth value **U (Undetermined)** represents **ontological indeterminacy** —
as in quantum mechanics before measurement. This is *not* epistemic ignorance or
lack of knowledge. U cannot be "resolved" by acquiring more information; it reflects
a genuine feature of reality prior to a σ-collapse event.

This distinguishes ECL₃^Q from standard K₃ extensions where the third value is
typically treated as "unknown" or "undefined."

## Quick Start

```bash
dotnet build
dotnet test
dotnet run --project ECL3Q.CLI -- --demo
dotnet run --project ECL3Q.Examples        # all examples
dotnet run --project ECL3Q.Examples -- 1   # quantum collapse
dotnet run --project ECL3Q.Examples -- 2   # legal judgment / σ-conflicts
dotnet run --project ECL3Q.Examples -- 3   # tableau completeness boundaries
```

## Project Structure

```
ECL3Q/
  ECL3Q.Core/
    Algebra/      TruthValue enum, Strong Kleene operators, Obs operator
    Syntax/       Formula AST, Parser (Unicode + ASCII fallbacks)
    Semantics/    World, Frame, Valuation, Model, Collapse, Entanglement,
                  DomainClassification (119 domains, SR401-rev3)
    Inference/    Tableau (propositional K₃), ModalTableau (full ECL₃^Q)
  ECL3Q.Agents/  Multi-agent frames, agent-specific observation relations
  ECL3Q.Taxonomy/ σ-Conflict taxonomy (8 categories, axioms Ax1–Ax7)
  ECL3Q.Examples/ Three worked examples (quantum, legal, tableau boundaries)
  ECL3Q.Tests/   Unit tests for all formal properties (87 tests)
  ECL3Q.CLI/     Interactive REPL
```

## Key Formal Results (Implemented as Tests)

| Result | Status | Test |
|--------|--------|------|
| OC-Lemma falsified | FALSIFIED | `AlgebraTests.OC_Countermodel_ShowsOCDoesNotHold` |
| OC-weak holds | PROVEN | `AlgebraTests.OCWeak_HoldsForCountermodel` |
| Obs not F₃-linear (Theorem A4) | THEOREM | Documented in `ObsOperator.cs` |
| F4 (linear representation) | OPEN | Marked as TODO, not implemented |
| LEM not K₃-valid | CONFIRMED | `InferenceTests.LEM_NotK3Valid_IsK3Valid_IsClassicalValid` |
| Classical logic contained | CONFIRMED | `SemanticTests.ClassicalTautologies_AreValidWhenUExcluded` |
| SC_PC as theorem (Option B) | THEOREM | `SemanticTests.SCPC_Holds_WhenSuperWorldHasEigenSuccessor` |
| RF1 frame-definable | CONFIRMED | `SemanticTests.RF1_ReflexiveFrame_SatisfiesRF1` |
| RF3 frame-definable | CONFIRMED | `SemanticTests.RF3_TransitiveFrame_SatisfiesRF3` |
| σ-Taxonomy: 8 categories | IMPLEMENTED | `ECL3Q.Taxonomy/SigmaConflict.cs` |

## Associated Papers

- **Paper I** (Core Logic, Semantics, Completeness): submitted to PhilArchive; target journal Studia Logica
  - [Preprint link — to be added upon availability]
- **Paper XVII** (σ-Conflict Taxonomy): target Studia Logica / J. Applied Logic
  - [Preprint link — to be added upon availability]

## Known Limitations and Open Problems

- **F4** (Open): Whether `Obs` admits a linear representation in a larger algebraic structure
- **Tableau completeness for U:Obs Source 2** (Open C): `U:Obs(φ)@w` arising from classical φ with disagreeing R_obs-successors is not handled. May produce false negatives for formulas requiring this as an intermediate step. Formulas with T:Obs and F:Obs are unaffected.
- **RF2 non-frame-definability**: Confirmed by two-model method; no axiom schema defines this frame class
- **Completeness theorem** for full ECL₃^Q (SC_PC + AO1_eigen frame class): not yet established; target Paper I §4.3

Note: `U:□φ`, `U:◇φ`, `U:σ(φ)`, `U:[do τ]φ` rules are **exact** (not approximations). The single-fresh-world witness is complete because U requires a U-valued successor and nothing else — any F-successor would give F, not U.

## AI Disclosure

Developed with Claude (Anthropic) as AI assistant for implementation, documentation,
and test construction. All concepts, formal results, proofs, and mathematical content
originate with **Markus Bauernfeind**.

## License

MIT — see [LICENSE](LICENSE).
