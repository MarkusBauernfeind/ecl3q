using ECL3Q.Core.Algebra;
using ECL3Q.Core.Inference;
using ECL3Q.Core.Semantics;
using ECL3Q.Core.Syntax;

namespace ECL3Q.Examples;

/// <summary>
/// Example 1: Quantum collapse scenario.
///
/// Models a particle in superposition (spin=U) that collapses to a definite
/// state (spin=T or spin=F) upon measurement.
///
/// Illustrates:
///   - W_super vs W_eigen distinction (Option B)
///   - σ-collapse operator
///   - Obs operator: before collapse, spin is not observable
///   - After collapse, spin is observable
/// </summary>
public static class Example1_QuantumCollapse
{
    public static void Run()
    {
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("Example 1: Quantum Collapse");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("Scenario: A particle with spin in ontological superposition.");
        Console.WriteLine("  w_super: spin = U  (W_super — genuine indeterminacy)");
        Console.WriteLine("  w_up:    spin = T  (W_eigen — post-collapse spin-up)");
        Console.WriteLine("  w_down:  spin = F  (W_eigen — post-collapse spin-down)");
        Console.WriteLine();

        // Worlds
        var wSuper = new World("w_super");
        var wUp    = new World("w_up");
        var wDown  = new World("w_down");

        // Frame:
        //   R_obs: w_super can observe w_up, w_down (measurement apparatus)
        //   R_τ:   collapse takes w_super to w_up or w_down
        var frame = new Frame(
            worlds: [wSuper, wUp, wDown],
            rObs: [(wSuper, wUp), (wSuper, wDown)],
            rTau: [(wSuper, wUp), (wSuper, wDown)]);

        // Valuation: "spin" is the action variable
        var val = new Valuation(
            assignments: [
                (("spin", wSuper), TruthValue.Undetermined),  // U = superposition
                (("spin", wUp),    TruthValue.True),           // T = spin up
                (("spin", wDown),  TruthValue.False)           // F = spin down
            ],
            actionVariables: ["spin"]);

        var model = new Model(frame, val);
        var spin = new Atom("spin");

        // ── World types ───────────────────────────────────────────────────────
        Console.WriteLine("World types (Option B: determined by V(τ,·)):");
        foreach (var w in new[] { wSuper, wUp, wDown })
            Console.WriteLine($"  {w.Id,-12} {model.GetWorldType(w)}");
        Console.WriteLine();

        // ── SC_PC verification ────────────────────────────────────────────────
        Console.WriteLine($"SC_PC holds (every W_super has W_eigen collapse target): " +
                          $"{model.VerifySC_PC()}");
        Console.WriteLine();

        // ── Obs before collapse ───────────────────────────────────────────────
        Console.WriteLine("Before collapse (at w_super):");
        var spinValue = model.Evaluate(spin, wSuper);
        var obsValue  = model.Evaluate(new ObsFormula(spin), wSuper);
        Console.WriteLine($"  V(spin, w_super)     = {spinValue}  " +
                          "(ontological indeterminacy — not ignorance)");
        Console.WriteLine($"  Obs(spin) at w_super = {obsValue}  " +
                          "(U: not observable pre-collapse)");
        Console.WriteLine();

        // ── σ-collapse ────────────────────────────────────────────────────────
        Console.WriteLine("σ-collapse (non-deterministic — two possible outcomes):");
        foreach (var (eigenWorld, value) in
                 CollapseOperator.CollapseAll(model, wSuper, spin))
        {
            var obsPost = model.Evaluate(new ObsFormula(spin), eigenWorld);
            Console.WriteLine($"  Collapse to {eigenWorld.Id,-12}: " +
                              $"spin = {value}, Obs(spin) = {obsPost}");
        }
        Console.WriteLine();

        // ── σ(spin) at w_super ────────────────────────────────────────────────
        var collapseFormula = new CollapseFormula(spin);
        var collapseValue = model.Evaluate(collapseFormula, wSuper);
        Console.WriteLine($"σ(spin) at w_super = {collapseValue}");
        Console.WriteLine("  (min over collapse targets: min(T,F) = F — both outcomes possible)");
        Console.WriteLine();

        // ── Entanglement extension ────────────────────────────────────────────
        Console.WriteLine("Entanglement extension (EPR-style):");
        var entangled = new EntangledModel(model);
        // Anti-correlated partner particle
        entangled.AddEntanglement(wSuper, wSuper, "spin",
            v => v == TruthValue.True ? TruthValue.False : TruthValue.True);

        var collapseUp = entangled.CollapseAndPropagate(wSuper, "spin", TruthValue.True);
        Console.WriteLine("  Particle A collapses to spin=T (up):");
        foreach (var r in collapseUp)
            Console.WriteLine($"    {r.World.Id}: spin = {r.Value}");
        Console.WriteLine();
        Console.WriteLine("  No-signaling holds pre-collapse: " +
                          $"{entangled.VerifyNoSignaling(wSuper, "spin")}");
    }
}
