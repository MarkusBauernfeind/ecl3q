using ECL3Q.Core.Algebra;
using ECL3Q.Core.Inference;
using ECL3Q.Core.Semantics;
using ECL3Q.Core.Syntax;
using static ECL3Q.Core.Syntax.F;

namespace ECL3Q.CLI;

/// <summary>
/// Interactive REPL for ECL₃^Q formula evaluation.
/// Usage: dotnet run
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ECL₃^Q Interactive System");
        Console.WriteLine("==========================");
        Console.WriteLine("Epistemic Causal Logic, Three-Valued, Quantum Extension");
        Console.WriteLine("Developed by Markus Bauernfeind | AI-assisted implementation by Claude (Anthropic)");
        Console.WriteLine();
        Console.WriteLine("Truth values: T (True), U (Undetermined/ontological), F (False)");
        Console.WriteLine("IMPORTANT: U is ONTOLOGICAL indeterminacy, not epistemic ignorance.");
        Console.WriteLine();

        if (args.Length > 0 && args[0] == "--demo")
        {
            RunDemo();
            return;
        }

        PrintHelp();

        while (true)
        {
            Console.Write("ECL3Q> ");
            var rawInput = Console.ReadLine()?.Trim();
            if (rawInput == null) return;
            var input = rawInput.ToLower();

            if (input is "exit" or "quit")
            {
                Console.WriteLine("Goodbye.");
                return;
            }
            else if (input == "demo") RunDemo();
            else if (input == "truth-tables") ShowTruthTables();
            else if (input == "oc-test") ShowOcCountermodel();
            else if (input == "k3-test") ShowK3VsClassical();
            else if (input == "sigma-taxonomy") ShowSigmaTaxonomy();
            else if (input is "help" or "") PrintHelp();
            else if (rawInput.StartsWith("valid ", StringComparison.OrdinalIgnoreCase))
                CheckValidity(rawInput[6..].Trim(), k3Only: true);
            else if (rawInput.StartsWith("classical ", StringComparison.OrdinalIgnoreCase))
                CheckValidity(rawInput[10..].Trim(), k3Only: false);
            else if (rawInput.StartsWith("parse ", StringComparison.OrdinalIgnoreCase))
                ParseAndShow(rawInput[6..].Trim());
            else
                Console.WriteLine($"Unknown command. Type 'help' for commands.");

            Console.WriteLine();
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  demo             — demonstration of core properties");
        Console.WriteLine("  truth-tables     — display all truth tables");
        Console.WriteLine("  oc-test          — OC countermodel (OC is falsified)");
        Console.WriteLine("  k3-test          — K3 vs classical tautology differences");
        Console.WriteLine("  sigma-taxonomy   — sigma-conflict taxonomy (8 categories)");
        Console.WriteLine("  valid <phi>      — check K3-validity  (e.g. valid (p -> p))");
        Console.WriteLine("  classical <phi>  — check classical validity");
        Console.WriteLine("  parse <phi>      — parse and display formula structure");
        Console.WriteLine("  help             — this help");
        Console.WriteLine("  exit             — quit");
        Console.WriteLine();
        Console.WriteLine("Formula syntax:");
        Console.WriteLine("  Atoms: p q r tau (any lowercase identifier)");
        Console.WriteLine("  ¬/!/~  (not)   ∧/&  (and)   ∨/|  (or)");
        Console.WriteLine("  →/->   (impl)  ↔/<-> (iff)");
        Console.WriteLine("  □/[]   (box)   ◇/<>  (diamond)");
        Console.WriteLine("  Obs(φ)  σ(φ)  [do action]φ");
        Console.WriteLine("  Binary connectives require parentheses: (p ∧ q)");
    }

    static void RunDemo()
    {
        Console.WriteLine("=== ECL₃^Q Core Properties Demo ===");
        Console.WriteLine();
        ShowTruthTables();
        ShowOcCountermodel();
        ShowK3VsClassical();
        ShowSigmaTaxonomy();
    }

    static void CheckValidity(string formulaStr, bool k3Only)
    {
        var formula = FormulaParser.TryParse(formulaStr, out var error);
        if (formula == null)
        {
            Console.WriteLine($"Parse error: {error}");
            return;
        }

        if (k3Only)
        {
            var k3Valid = ProofSearch.IsK3Tautology(formula);
            Console.WriteLine($"K₃-valid: {k3Valid}");
            if (!k3Valid)
            {
                // Find a countermodel
                var atoms = formula.Atoms().ToList();
                var counter = ProofSearch.EnumerateAssignments(atoms)
                    .FirstOrDefault(a => ProofSearch.EvaluatePropositional(formula, a) != TruthValue.True);
                if (counter != null)
                {
                    Console.WriteLine($"Counterassignment: {string.Join(", ", counter.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    Console.WriteLine($"  Result: {ProofSearch.EvaluatePropositional(formula, counter)}");
                }
            }
        }
        else
        {
            var classValid = ProofSearch.IsClassicalTautology(formula);
            Console.WriteLine($"Classically valid: {classValid}");
            if (!classValid)
            {
                var atoms = formula.Atoms().ToList();
                var counter = ProofSearch.EnumerateClassicalAssignments(atoms)
                    .FirstOrDefault(a => ProofSearch.EvaluatePropositional(formula, a) != TruthValue.True);
                if (counter != null)
                {
                    Console.WriteLine($"Counterassignment: {string.Join(", ", counter.Select(kv => $"{kv.Key}={kv.Value}"))}");
                }
            }
        }
    }

    static void ParseAndShow(string formulaStr)
    {
        var formula = FormulaParser.TryParse(formulaStr, out var error);
        if (formula == null)
        {
            Console.WriteLine($"Parse error: {error}");
            return;
        }
        Console.WriteLine($"Parsed:  {formula}");
        Console.WriteLine($"Type:    {formula.GetType().Name}");
        Console.WriteLine($"Depth:   {formula.Depth()}");
        var atoms = formula.Atoms().ToList();
        if (atoms.Any())
            Console.WriteLine($"Atoms:   {string.Join(", ", atoms)}");
    }

    static void ShowTruthTables()
    {
        Console.WriteLine("--- Truth Tables (Strong Kleene) ---");
        Console.WriteLine();

        Console.WriteLine("Negation (¬):");
        Console.WriteLine("  A   ¬A");
        foreach (var a in AllValues())
            Console.WriteLine($"  {a,-3} {Operators.Not(a),-3}");
        Console.WriteLine();

        Console.WriteLine("Conjunction (∧) = min:");
        Console.WriteLine("  A   B   A∧B");
        foreach (var a in AllValues())
            foreach (var b in AllValues())
                Console.WriteLine($"  {a,-3} {b,-3} {Operators.And(a, b),-3}");
        Console.WriteLine();

        Console.WriteLine("Disjunction (∨) = max:");
        Console.WriteLine("  A   B   A∨B");
        foreach (var a in AllValues())
            foreach (var b in AllValues())
                Console.WriteLine($"  {a,-3} {b,-3} {Operators.Or(a, b),-3}");
        Console.WriteLine();

        Console.WriteLine("Implication (→) = max(¬A, B):");
        Console.WriteLine("  A   B   A→B");
        foreach (var a in AllValues())
            foreach (var b in AllValues())
                Console.WriteLine($"  {a,-3} {b,-3} {Operators.Implies(a, b),-3}");
        Console.WriteLine();
    }

    static void ShowOcCountermodel()
    {
        Console.WriteLine("--- OC Countermodel (OC is FALSIFIED) ---");
        Console.WriteLine();
        Console.WriteLine("OC (claimed): obs_w(A∧B) = min(obs_w(A), obs_w(B))");
        Console.WriteLine("Status: FALSIFIED via countermodel (SR_basis)");
        Console.WriteLine();

        var (obsConj, obsA, obsB) = ECL3Q.Core.Algebra.ObsOperator.GetOcCountermodelValues();
        Console.WriteLine("Countermodel values:");
        Console.WriteLine($"  obs_w(A)    = {obsA}");
        Console.WriteLine($"  obs_w(B)    = {obsB}");
        Console.WriteLine($"  obs_w(A∧B)  = {obsConj}");
        Console.WriteLine($"  min(obs(A), obs(B)) = {(TruthValue)Math.Min((int)obsA, (int)obsB)}");
        Console.WriteLine();

        var isOCViolated = obsConj != (TruthValue)Math.Min((int)obsA, (int)obsB);
        var isOCWeakHeld = ECL3Q.Core.Algebra.ObsOperator.VerifyOcWeak(obsConj, obsA, obsB);

        Console.WriteLine($"OC (strong) violated: {isOCViolated}  <- expected: True");
        Console.WriteLine($"OC-weak holds:        {isOCWeakHeld}  <- expected: True");
        Console.WriteLine();
        Console.WriteLine("Note: Obs is NOT F3-linear (Theorem A4). No matrix representation.");
        Console.WriteLine("      F4 (linear representation in larger structure) remains OPEN.");
        Console.WriteLine();
    }

    static void ShowK3VsClassical()
    {
        Console.WriteLine("--- K₃ vs Classical Tautologies ---");
        Console.WriteLine();

        var tests = new (string Name, Formula Formula)[]
        {
            ("LEM (p∨¬p)", Or(F.P, Not(F.P))),
            ("Non-contradiction ¬(p∧¬p)", Not(And(F.P, Not(F.P)))),
            ("Double negation ¬¬p↔p", Iff(Not(Not(F.P)), F.P)),
            ("Conj elimination p∧q→p", Implies(And(F.P, F.Q), F.P)),
            ("Disj introduction p→p∨q", Implies(F.P, Or(F.P, F.Q))),
            ("Modus ponens (p∧(p→q))→q", Implies(And(F.P, Implies(F.P, F.Q)), F.Q)),
        };

        Console.WriteLine($"  {"Formula",-35} {"K3-valid",-12} {"Classically valid"}");
        Console.WriteLine($"  {new string('-', 62)}");

        foreach (var (name, formula) in tests)
        {
            var k3 = ProofSearch.IsK3Tautology(formula);
            var classical = ProofSearch.IsClassicalTautology(formula);
            Console.WriteLine($"  {name,-35} {k3,-12} {classical}");
        }
        Console.WriteLine();
        Console.WriteLine("LEM and Non-Contradiction are NOT K3-valid (U is a genuine third value).");
        Console.WriteLine("ECL3Q properly contains classical logic when U is excluded.");
        Console.WriteLine();
    }

    static void ShowSigmaTaxonomy()
    {
        Console.WriteLine("--- sigma-Conflict Taxonomy (8 categories, SR1016) ---");
        Console.WriteLine();
        Console.WriteLine("Formal axiomatization: Ax1-Ax7");
        Console.WriteLine("Minimality proven: exactly 5 minimal 4-dimension classification systems.");
        Console.WriteLine();

        var categories = new[]
        {
            ("sigma-Hierarchie-Konflikt", "Competing collapse hierarchies — multiple agents claim carrier authority"),
            ("sigma-Vakuum",              "No legitimate sigma-carrier present for a W_super world"),
            ("sigma-Inversion",           "Collapse in wrong direction: W_eigen -> W_super (irreversibility violated)"),
            ("sigma-Unterwanderung",       "Carrier exists but no actual collapse occurs"),
            ("sigma-Vorab-Kollaps",        "Collapse before legitimate observation preconditions are met"),
            ("sigma-Delegation",           "Unauthorized transfer of sigma-carrier status"),
            ("sigma-Kaskade",              "Propagation of collapse errors through the system"),
            ("sigma-Diskontinuitaet",      "Broken collapse chain, no path to W_eigen"),
        };

        for (int i = 0; i < categories.Length; i++)
            Console.WriteLine($"  {i + 1}. {categories[i].Item1,-28} {categories[i].Item2}");

        Console.WriteLine();
        Console.WriteLine("Ax4 (Disjointness): each sigma-conflict belongs to exactly one category.");
        Console.WriteLine("Ax5 (Completeness): every sigma-conflict falls into some category.");
        Console.WriteLine();
    }

    static TruthValue[] AllValues() =>
        [TruthValue.False, TruthValue.Undetermined, TruthValue.True];
}
