# ECL₃^Q — Epistemic Causal Logic (Three-Valued, Quantum Extension)

A formal logic system with three truth values (T/U/F) where U denotes 
ontological indeterminacy — not epistemic uncertainty.

Developed by Markus Bauernfeind. Implemented in C# (.NET 8).

## Core Concepts

- **T / U / F** — True, Undetermined (ontological), False
- **Obs operator** — Observability operator (not F₃-linear, open problem F4)
- **σ-collapse** — Collapse operator from superposition world to eigenstate
- **[do τ]** — Dynamic operator for collapse dynamics
- **Multi-agent** — Individual observability relations and σ-authority
- **σ-conflict taxonomy** — 8 categories (Ax1–Ax7), proven minimal and complete

## Projects

| Project | Content |
|---|---|
| ECL3Q.Core | Algebra, Syntax, Semantics, Inference (Tableau) |
| ECL3Q.Agents | Multi-agent frames, conflict detection |
| ECL3Q.Taxonomy | σ-conflict taxonomy (Ax1–Ax7) |
| ECL3Q.Examples | Worked examples |
| ECL3Q.Tests | Unit tests |
| ECL3Q.CLI | Interactive REPL |

## Quick Start

dotnet build
dotnet test
dotnet run --project ECL3Q.CLI

## Publications

Paper I (ECL₃^Q formal system): forthcoming — Studia Logica (in preperstion)
Paper XVII (σ-conflict taxonomy): in preparation
Preprint: link to be added upon availability (arXiv / PhilArchive)

## AI Disclosure

Developed with Claude (Anthropic) as AI assistant for implementation 
and formalization. All concepts, proofs, and mathematical content 
originate with Markus Bauernfeind.

## License

MIT License — see LICENSE file.
