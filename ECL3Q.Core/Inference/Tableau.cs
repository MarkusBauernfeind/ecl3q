using ECL3Q.Core.Algebra;
using ECL3Q.Core.Syntax;

namespace ECL3Q.Core.Inference;

/// <summary>
/// Truth-value sign for tableau nodes.
/// T = "assumed true", F = "assumed false", U = "assumed undetermined".
/// </summary>
public enum Sign { T, F, U }

/// <summary>A formula annotated with a truth-value assumption.</summary>
public record SignedFormula(Formula Formula, Sign Sign)
{
    public override string ToString() => $"{Sign}:{Formula}";
}

/// <summary>
/// A single branch in a propositional tableau.
/// Closed iff it contains T:φ and F:φ for the same formula φ.
/// T+U and F+U do NOT close a branch.
/// </summary>
public class TableauBranch
{
    private readonly List<SignedFormula> _formulas = [];
    private readonly HashSet<SignedFormula> _formulaSet = [];
    private readonly HashSet<SignedFormula> _expanded = [];

    public IReadOnlyList<SignedFormula> Formulas => _formulas;

    public TableauBranch(IEnumerable<SignedFormula>? initial = null)
    {
        if (initial == null) return;
        foreach (var sf in initial) Add(sf);
    }

    public TableauBranch Clone()
    {
        var b = new TableauBranch();
        foreach (var sf in _formulas) { b._formulas.Add(sf); b._formulaSet.Add(sf); }
        b._expanded.UnionWith(_expanded);
        return b;
    }

    /// <summary>
    /// Returns a deep copy with <paramref name="exclude"/> removed and marked expanded.
    /// Used by Split rules so the source formula is not re-expanded in child branches.
    /// </summary>
    public TableauBranch CloneWithout(SignedFormula exclude)
    {
        var b = new TableauBranch();
        foreach (var sf in _formulas)
        {
            if (sf == exclude) continue;
            b._formulas.Add(sf);
            b._formulaSet.Add(sf);
        }
        b._expanded.UnionWith(_expanded);
        return b;
    }

    public void Add(SignedFormula sf)
    {
        if (_formulaSet.Add(sf)) _formulas.Add(sf);
    }

    public bool Contains(SignedFormula sf) => _formulaSet.Contains(sf);

    /// <summary>Marks sf as expanded so it is not re-processed in subsequent iterations.</summary>
    public void MarkExpanded(SignedFormula sf) => _expanded.Add(sf);

    public bool IsExpanded(SignedFormula sf) => _expanded.Contains(sf);

    public bool IsClosed() =>
        _formulas.Any(sf =>
            sf.Sign == Sign.T &&
            _formulaSet.Contains(sf with { Sign = Sign.F }));
}

/// <summary>
/// Propositional analytic tableau for ECL₃^Q.
///
/// SEMANTICS: Checks CLASSICAL validity (refutes F:φ).
/// A K₃ countermodel has V(φ)=U, not F — so this does not check K₃-validity.
/// For K₃-validity use <see cref="ProofSearch.IsK3Tautology"/>.
///
/// Each formula is expanded at most once per branch (tracked via _expanded set).
/// Split rules remove the source formula from child branches via CloneWithout.
/// </summary>
public class Tableau
{
    private readonly List<TableauBranch> _branches;

    public Tableau(Formula formula)
    {
        var initial = new TableauBranch([new SignedFormula(formula, Sign.F)]);
        _branches = [initial];
    }

    /// <summary>Returns true iff φ is classically valid.</summary>
    public static bool IsValid(Formula formula, int maxDepth = 200)
    {
        var tableau = new Tableau(formula);
        return tableau.Expand(maxDepth);
    }

    public bool Expand(int maxDepth)
    {
        for (int i = 0; i < maxDepth; i++)
        {
            bool progress = false;
            foreach (var branch in _branches.Where(b => !b.IsClosed()).ToList())
            {
                var (expanded, newBranches) = ExpandBranch(branch);
                if (newBranches.Count > 0)
                {
                    _branches.Remove(branch);
                    _branches.AddRange(newBranches);
                }
                progress |= expanded;
            }
            if (!progress) break;
        }
        return _branches.All(b => b.IsClosed());
    }

    private static (bool expanded, List<TableauBranch> newBranches) ExpandBranch(
        TableauBranch branch)
    {
        foreach (var sf in branch.Formulas.ToList())
        {
            if (branch.IsExpanded(sf)) continue;

            switch (sf)
            {
                // ── Negation (all signs, linear) ─────────────────────────────
                case { Formula: Negation n, Sign: Sign.T }:
                    return Linear(branch, sf, new SignedFormula(n.Sub, Sign.F));
                case { Formula: Negation n, Sign: Sign.F }:
                    return Linear(branch, sf, new SignedFormula(n.Sub, Sign.T));
                case { Formula: Negation n, Sign: Sign.U }:
                    return Linear(branch, sf, new SignedFormula(n.Sub, Sign.U));

                // ── Conjunction: T (linear) ───────────────────────────────────
                case { Formula: Conjunction c, Sign: Sign.T }:
                {
                    branch.MarkExpanded(sf);
                    bool added = false;
                    added |= AddTo(branch, new SignedFormula(c.Left,  Sign.T));
                    added |= AddTo(branch, new SignedFormula(c.Right, Sign.T));
                    if (added) return (true, []);
                    break;
                }

                // ── Conjunction: F (branching) ────────────────────────────────
                case { Formula: Conjunction c, Sign: Sign.F }:
                    return Split2(branch, sf,
                        new SignedFormula(c.Left,  Sign.F),
                        new SignedFormula(c.Right, Sign.F));

                // ── Conjunction: U (branching) ────────────────────────────────
                case { Formula: Conjunction c, Sign: Sign.U }:
                    return Split3(branch, sf,
                        [new(c.Left, Sign.U), new(c.Right, Sign.T)],
                        [new(c.Left, Sign.T), new(c.Right, Sign.U)],
                        [new(c.Left, Sign.U), new(c.Right, Sign.U)]);

                // ── Disjunction: T (branching) ────────────────────────────────
                case { Formula: Disjunction d, Sign: Sign.T }:
                    return Split2(branch, sf,
                        new SignedFormula(d.Left,  Sign.T),
                        new SignedFormula(d.Right, Sign.T));

                // ── Disjunction: F (linear) ───────────────────────────────────
                case { Formula: Disjunction d, Sign: Sign.F }:
                {
                    branch.MarkExpanded(sf);
                    bool added = false;
                    added |= AddTo(branch, new SignedFormula(d.Left,  Sign.F));
                    added |= AddTo(branch, new SignedFormula(d.Right, Sign.F));
                    if (added) return (true, []);
                    break;
                }

                // ── Disjunction: U (branching) ────────────────────────────────
                case { Formula: Disjunction d, Sign: Sign.U }:
                    return Split3(branch, sf,
                        [new(d.Left, Sign.U), new(d.Right, Sign.F)],
                        [new(d.Left, Sign.F), new(d.Right, Sign.U)],
                        [new(d.Left, Sign.U), new(d.Right, Sign.U)]);

                // ── Implication: T (branching) ────────────────────────────────
                case { Formula: Implication i, Sign: Sign.T }:
                    return Split2(branch, sf,
                        new SignedFormula(i.Antecedent, Sign.F),
                        new SignedFormula(i.Consequent, Sign.T));

                // ── Implication: F (linear) ───────────────────────────────────
                case { Formula: Implication i, Sign: Sign.F }:
                {
                    branch.MarkExpanded(sf);
                    bool added = false;
                    added |= AddTo(branch, new SignedFormula(i.Antecedent, Sign.T));
                    added |= AddTo(branch, new SignedFormula(i.Consequent, Sign.F));
                    if (added) return (true, []);
                    break;
                }

                // ── Implication: U (branching) ────────────────────────────────
                case { Formula: Implication i, Sign: Sign.U }:
                    return Split3(branch, sf,
                        [new(i.Antecedent, Sign.U), new(i.Consequent, Sign.U)],
                        [new(i.Antecedent, Sign.T), new(i.Consequent, Sign.U)],
                        [new(i.Antecedent, Sign.U), new(i.Consequent, Sign.F)]);

                // ── Biconditional: T (linear) ─────────────────────────────────
                case { Formula: Biconditional b, Sign: Sign.T }:
                {
                    branch.MarkExpanded(sf);
                    bool added = false;
                    added |= AddTo(branch, new SignedFormula(new Implication(b.Left,  b.Right), Sign.T));
                    added |= AddTo(branch, new SignedFormula(new Implication(b.Right, b.Left),  Sign.T));
                    if (added) return (true, []);
                    break;
                }

                // ── Biconditional: F (branching) ──────────────────────────────
                case { Formula: Biconditional b, Sign: Sign.F }:
                    return Split2(branch, sf,
                        new SignedFormula(new Implication(b.Left,  b.Right), Sign.F),
                        new SignedFormula(new Implication(b.Right, b.Left),  Sign.F));

                // ── Biconditional: U (branching) ──────────────────────────────
                case { Formula: Biconditional b, Sign: Sign.U }:
                {
                    var ab = new Implication(b.Left, b.Right);
                    var ba = new Implication(b.Right, b.Left);
                    return Split3(branch, sf,
                        [new(ab, Sign.U), new(ba, Sign.T)],
                        [new(ab, Sign.T), new(ba, Sign.U)],
                        [new(ab, Sign.U), new(ba, Sign.U)]);
                }

                // ── Modal operators: use ModalTableau instead ─────────────────
                case { Formula: ModalBox or ModalDiamond or ObsFormula
                              or CollapseFormula or DoOperator }:
                    branch.MarkExpanded(sf);
                    break;
            }
        }
        return (false, []);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (bool, List<TableauBranch>) Linear(
        TableauBranch branch, SignedFormula source, SignedFormula target)
    {
        branch.MarkExpanded(source);
        if (!branch.Contains(target)) { branch.Add(target); return (true, []); }
        return (false, []);
    }

    private static bool AddTo(TableauBranch branch, SignedFormula sf)
    {
        if (branch.Contains(sf)) return false;
        branch.Add(sf);
        return true;
    }

    private static (bool, List<TableauBranch>) Split2(
        TableauBranch branch, SignedFormula source,
        SignedFormula left, SignedFormula right)
    {
        var b1 = branch.CloneWithout(source); b1.Add(left);
        var b2 = branch.CloneWithout(source); b2.Add(right);
        return (true, [b1, b2]);
    }

    private static (bool, List<TableauBranch>) Split3(
        TableauBranch branch, SignedFormula source,
        IReadOnlyList<SignedFormula> adds1,
        IReadOnlyList<SignedFormula> adds2,
        IReadOnlyList<SignedFormula> adds3)
    {
        var b1 = branch.CloneWithout(source); foreach (var f in adds1) b1.Add(f);
        var b2 = branch.CloneWithout(source); foreach (var f in adds2) b2.Add(f);
        var b3 = branch.CloneWithout(source); foreach (var f in adds3) b3.Add(f);
        return (true, [b1, b2, b3]);
    }
}

/// <summary>
/// Higher-level proof interface for ECL₃^Q propositional fragment.
/// For modal/Obs/σ formulas use <see cref="ModalTableau"/>.
/// </summary>
public static class ProofSearch
{
    /// <summary>
    /// Returns true iff φ evaluates to T under all three-valued (T/U/F) assignments.
    /// Sound and complete for propositional K₃.
    ///
    /// META-RESULT (L Research 17, confirmed): For purely Boolean schemas (∧,∨,¬,→,↔
    /// without ECL₃^Q-specific operators Obs/σ/[do τ]/U-constants), setting all atoms
    /// to U yields U for every formula — so this method returns false for all purely
    /// Boolean schemas. IsK3Tautology is non-trivial only for formulas involving
    /// ECL₃^Q-specific operators where U-propagation can be interrupted.
    ///
    /// For the weaker property "never false" use <see cref="IsNeverFalse"/>.
    /// </summary>
    public static bool IsK3Tautology(Formula formula)
    {
        var atoms = formula.Atoms().ToList();
        return EnumerateAssignments(atoms)
            .All(a => EvaluatePropositional(formula, a) == TruthValue.True);
    }

    /// <summary>
    /// Returns true iff φ never evaluates to F under any three-valued assignment.
    /// This is the weak designated-value property for K₃ (value ∈ {T, U} always).
    ///
    /// Unlike <see cref="IsK3Tautology"/> (always T), this has non-trivial instances
    /// in the purely Boolean fragment:
    ///   p ∨ ¬p  — always T or U, never F  ✓
    ///   p → p   — always T or U, never F  ✓
    ///   p ∧ ¬p  — F when p=T or p=F      ✗
    /// </summary>
    public static bool IsNeverFalse(Formula formula)
    {
        var atoms = formula.Atoms().ToList();
        return EnumerateAssignments(atoms)
            .All(a => EvaluatePropositional(formula, a) != TruthValue.False);
    }

    /// <summary>Returns true iff φ is classically valid (T under all T/F assignments).</summary>
    public static bool IsClassicalTautology(Formula formula)
    {
        var atoms = formula.Atoms().ToList();
        return EnumerateClassicalAssignments(atoms)
            .All(a => EvaluatePropositional(formula, a) == TruthValue.True);
    }

    public static IEnumerable<Dictionary<string, TruthValue>> EnumerateAssignments(
        IList<string> atoms) =>
        Enumerate(atoms, 0, new Dictionary<string, TruthValue>(),
            [TruthValue.False, TruthValue.Undetermined, TruthValue.True]);

    public static IEnumerable<Dictionary<string, TruthValue>> EnumerateClassicalAssignments(
        IList<string> atoms) =>
        Enumerate(atoms, 0, new Dictionary<string, TruthValue>(),
            [TruthValue.False, TruthValue.True]);

    private static IEnumerable<Dictionary<string, TruthValue>> Enumerate(
        IList<string> atoms, int index,
        Dictionary<string, TruthValue> current,
        TruthValue[] values)
    {
        if (index == atoms.Count)
        {
            yield return new Dictionary<string, TruthValue>(current);
            yield break;
        }
        foreach (var v in values)
        {
            current[atoms[index]] = v;
            foreach (var assignment in Enumerate(atoms, index + 1, current, values))
                yield return assignment;
        }
    }

    public static TruthValue EvaluatePropositional(
        Formula formula, Dictionary<string, TruthValue> assignment) => formula switch
    {
        Atom a          => assignment.TryGetValue(a.Name, out var v) ? v : TruthValue.False,
        Negation n      => Operators.Not(EvaluatePropositional(n.Sub, assignment)),
        Conjunction c   => Operators.And(EvaluatePropositional(c.Left,  assignment),
                                         EvaluatePropositional(c.Right, assignment)),
        Disjunction d   => Operators.Or(EvaluatePropositional(d.Left,   assignment),
                                        EvaluatePropositional(d.Right,  assignment)),
        Implication i   => Operators.Implies(EvaluatePropositional(i.Antecedent, assignment),
                                             EvaluatePropositional(i.Consequent, assignment)),
        Biconditional b => Operators.Iff(EvaluatePropositional(b.Left,  assignment),
                                         EvaluatePropositional(b.Right, assignment)),
        _ => throw new NotSupportedException(
            $"Propositional evaluation does not support {formula.GetType().Name}. " +
            "Use Model.Evaluate for modal/Obs/σ formulas.")
    };
}
