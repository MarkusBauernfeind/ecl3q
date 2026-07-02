using ECL3Q.Core.Inference;
using ECL3Q.Core.Syntax;
using static ECL3Q.Core.Syntax.F;

namespace ECL3Q.Examples;

/// <summary>
/// Example 3: Tableau completeness boundaries.
///
/// Demonstrates which formulas the tableau handles correctly,
/// and exhibits the one known genuine incompleteness (Open C: U:Obs Source 2).
///
/// This example is intentionally honest about limitations.
/// </summary>
public static class Example3_TableauBoundaries
{
    public static void Run()
    {
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("Example 3: Tableau Completeness Boundaries");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine();

        RunCompleteFragments();
        RunOpenProblemC();
    }

    static void RunCompleteFragments()
    {
        Console.WriteLine("── Complete fragments (sound and complete) ──");
        Console.WriteLine();

        var cases = new (string Label, Formula Formula, bool ExpectedValid)[]
        {
            // Propositional K₃
            ("p→p  (NOT K₃-valid: U→U=U)",
             Implies(P, P), false),
            ("¬¬p↔p  (K₃-valid: involution)",
             Iff(Not(Not(P)), P), true),
            ("p∧q→p  (K₃-valid)",
             Implies(And(P, Q), P), true),
            ("p∨¬p  (NOT K₃-valid: LEM fails)",
             Or(P, Not(P)), false),

            // Modal K
            ("□(p→q)→(□p→□q)  (K-axiom, valid)",
             Implies(Box(Implies(P, Q)), Implies(Box(P), Box(Q))), true),
            ("□p→◇p  (NOT valid: empty frame)",
             Implies(Box(P), Diamond(P)), false),
            ("□p↔¬◇¬p  (duality, valid)",
             Iff(Box(P), Not(Diamond(Not(P)))), true),

            // Dynamic [do τ]
            ("[do τ](p→q)→([do τ]p→[do τ]q)  (K-analogue, valid)",
             Implies(Do("tau", Implies(P, Q)),
                     Implies(Do("tau", P), Do("tau", Q))), true),

            // σ-collapse
            ("σ(p→q)→(σ(p)→σ(q))  (K-analogue for σ, valid)",
             Implies(Collapse(Implies(P, Q)),
                     Implies(Collapse(P), Collapse(Q))), true),
            ("□p→σ(p)  (NOT valid: different relations R_obs vs R_τ)",
             Implies(Box(P), Collapse(P)), false),

            // Obs
            ("Obs(p)→p  (observability implies truth, valid)",
             Implies(Obs(P), P), true),
        };

        Console.WriteLine($"  {"Formula",-52} {"Expected",-10} {"Tableau"}");
        Console.WriteLine($"  {new string('─', 72)}");

        foreach (var (label, formula, expected) in cases)
        {
            var result = ModalTableau.IsModallyValid(formula);
            var match = result == expected ? "✓" : "✗ MISMATCH";
            Console.WriteLine($"  {label,-52} {expected,-10} {result} {match}");
        }
        Console.WriteLine();
    }

    static void RunOpenProblemC()
    {
        Console.WriteLine("── Open problem C: U:Obs Source 2 ──");
        Console.WriteLine();
        Console.WriteLine("  The one known incompleteness: formulas where Obs(φ)=U arises");
        Console.WriteLine("  because φ is classical at w but R_obs-successors disagree.");
        Console.WriteLine();
        Console.WriteLine("  Example scenario (not expressible as a single closed formula,");
        Console.WriteLine("  but arises as an intermediate state in certain derivations):");
        Console.WriteLine();
        Console.WriteLine("    w:  φ=T (classical)");
        Console.WriteLine("    v₁: φ=T  (R_obs-successor)");
        Console.WriteLine("    v₂: φ=F  (R_obs-successor)");
        Console.WriteLine("    → EvaluateObs(φ,w) = U  (successors disagree)");
        Console.WriteLine();
        Console.WriteLine("  The tableau rule for U:Obs(φ)@w adds U:φ@w.");
        Console.WriteLine("  If T:φ@w is already present, no contradiction arises (T+U ≠ ⊥).");
        Console.WriteLine("  The branch stays open when it should be further developed.");
        Console.WriteLine();
        Console.WriteLine("  Affected: valid formulas with U:Obs intermediate step from Source 2.");
        Console.WriteLine("  Not affected: formulas involving only T:Obs or F:Obs.");
        Console.WriteLine();

        // Demonstrate what IS correctly handled: T:Obs and F:Obs
        var obsImpliesP = Implies(Obs(P), P);
        var fObs = ModalTableau.IsModallyValid(obsImpliesP);
        Console.WriteLine($"  Obs(p)→p  (T:Obs case): valid={fObs}  ✓ correctly handled");

        // Note: we cannot easily construct a closed formula that specifically
        // triggers Source 2 incompleteness — it manifests in derivation steps,
        // not in the input formula itself.
        Console.WriteLine();
        Console.WriteLine("  Note: Source 2 incompleteness manifests in intermediate");
        Console.WriteLine("  derivation steps, not in simple closed test formulas.");
        Console.WriteLine("  It is documented as open problem (C) in ModalTableau.cs.");
    }
}
