using ECL3Q.Core.Algebra;
using ECL3Q.Core.Semantics;
using ECL3Q.Core.Syntax;
using ECL3Q.Agents;
using ECL3Q.Taxonomy;

namespace ECL3Q.Examples;

/// <summary>
/// Example 2: Legal judgment as σ-collapse (Law domain, SR117–SR138).
///
/// A legal dispute is in ontological superposition until a court issues
/// a binding judgment. The court is the σ-carrier.
///
/// Demonstrates:
///   - σ-Vakuum: no court has jurisdiction (no σ-carrier)
///   - σ-Hierarchie-Konflikt: two courts both claim jurisdiction
///   - σ-Delegation: court delegates to unauthorized body
///   - Well-formed case: exactly one court, proper collapse
///   - σ-Taxonomie conflict detector (Ax1–Ax7)
/// </summary>
public static class Example2_LegalJudgment
{
    public static void Run()
    {
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine("Example 2: Legal Judgment as σ-Collapse");
        Console.WriteLine("══════════════════════════════════════════════════");
        Console.WriteLine();

        RunWellFormedCase();
        RunVakuumCase();
        RunHierarchieKonfliktCase();
    }

    static void RunWellFormedCase()
    {
        Console.WriteLine("── Case A: Well-formed (one court, proper collapse) ──");
        Console.WriteLine();

        var wDispute  = new World("w_dispute");   // W_super: liability = U
        var wLiable   = new World("w_liable");    // W_eigen: liable = T
        var wNotLiable = new World("w_not_liable"); // W_eigen: liable = F

        var frame = new Frame(
            worlds: [wDispute, wLiable, wNotLiable],
            rObs: [(wDispute, wLiable), (wDispute, wNotLiable)],
            rTau: [(wDispute, wLiable), (wDispute, wNotLiable)]);

        var val = new Valuation(
            assignments: [
                (("liable", wDispute),   TruthValue.Undetermined),
                (("liable", wLiable),    TruthValue.True),
                (("liable", wNotLiable), TruthValue.False)
            ],
            actionVariables: ["liable"]);

        var model = new Model(frame, val);
        var court = new Agent("SupremeCourt", isSigmaCarrier: true);
        var maFrame = new MultiAgentFrame(frame, [court]);

        var detector = new SigmaConflictDetector(maFrame, model);
        var conflicts = detector.DetectAll();

        Console.WriteLine($"  SC_PC holds: {model.VerifySC_PC()}");
        Console.WriteLine($"  σ-conflicts detected: {conflicts.Count}");
        Console.WriteLine("  → Well-formed: court can issue binding judgment.");
        Console.WriteLine();
    }

    static void RunVakuumCase()
    {
        Console.WriteLine("── Case B: σ-Vakuum (no court has jurisdiction) ──");
        Console.WriteLine();

        var wDispute = new World("w_dispute");
        var wResolved = new World("w_resolved");

        var frame = new Frame(
            worlds: [wDispute, wResolved],
            rObs: [(wDispute, wResolved)],
            rTau: [(wDispute, wResolved)]);

        var val = new Valuation(
            assignments: [
                (("liable", wDispute),  TruthValue.Undetermined),
                (("liable", wResolved), TruthValue.True)
            ],
            actionVariables: ["liable"]);

        var model = new Model(frame, val);
        // No agents — no σ-carrier
        var maFrame = new MultiAgentFrame(frame, []);

        var detector = new SigmaConflictDetector(maFrame, model);
        var conflicts = detector.DetectAll();

        Console.WriteLine($"  σ-conflicts detected: {conflicts.Count}");
        foreach (var c in conflicts)
            Console.WriteLine($"  [{c.Category}] {c.Description}");
        Console.WriteLine("  → No institution has authority to decide. Legal vacuum.");
        Console.WriteLine();
    }

    static void RunHierarchieKonfliktCase()
    {
        Console.WriteLine("── Case C: σ-Hierarchie-Konflikt (two courts claim jurisdiction) ──");
        Console.WriteLine();

        var wDispute = new World("w_dispute");
        var wFederal = new World("w_federal_decision");
        var wState   = new World("w_state_decision");

        var frame = new Frame(
            worlds: [wDispute, wFederal, wState],
            rObs: [(wDispute, wFederal), (wDispute, wState)],
            rTau: [(wDispute, wFederal), (wDispute, wState)]);

        var val = new Valuation(
            assignments: [
                (("liable", wDispute), TruthValue.Undetermined),
                (("liable", wFederal), TruthValue.True),
                (("liable", wState),   TruthValue.False)
            ],
            actionVariables: ["liable"]);

        var model = new Model(frame, val);

        // Two courts, each claims to be σ-carrier, different observation relations
        var federalCourt = new Agent("FederalCourt", isSigmaCarrier: true,
            individualRObs: [(wDispute, wFederal)]);
        var stateCourt = new Agent("StateCourt", isSigmaCarrier: true,
            individualRObs: [(wDispute, wState)]);

        var maFrame = new MultiAgentFrame(frame, [federalCourt, stateCourt]);

        var detector = new SigmaConflictDetector(maFrame, model);
        var conflicts = detector.DetectAll();

        Console.WriteLine($"  σ-conflicts detected: {conflicts.Count}");
        foreach (var c in conflicts)
        {
            var agents = c.InvolvedAgents != null
                ? $" [{string.Join(", ", c.InvolvedAgents.Select(a => a.Id))}]"
                : "";
            Console.WriteLine($"  [{c.Category}]{agents} {c.Description}");
        }
        Console.WriteLine("  → Jurisdictional conflict: both courts reach contradictory decisions.");
        Console.WriteLine("    Federal: liable=T. State: liable=F.");
        Console.WriteLine();
    }
}
