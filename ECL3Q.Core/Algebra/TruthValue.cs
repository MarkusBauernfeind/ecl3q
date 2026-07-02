namespace ECL3Q.Core.Algebra;

/// <summary>
/// The three truth values of ECL₃^Q.
///
/// CRITICAL SEMANTIC DISTINCTION:
/// U (Undetermined) represents ONTOLOGICAL indeterminacy — as in quantum mechanics
/// before measurement/collapse. This is NOT epistemic ignorance or lack of knowledge.
/// The system cannot have U "resolved" by acquiring more information; U is a genuine
/// feature of reality prior to a σ-collapse event.
///
/// This distinction from standard K₃ (Kleene's strong three-valued logic) is fundamental.
/// See Paper I §1, Paper II §2 for formal grounding.
/// </summary>
public enum TruthValue
{
    /// <summary>False (0). Classical falsehood.</summary>
    False = 0,

    /// <summary>
    /// Ontologically Undetermined (1). Not "unknown" — genuinely indeterminate.
    /// Corresponds to quantum superposition state prior to σ-collapse.
    /// </summary>
    Undetermined = 1,

    /// <summary>True (2). Classical truth.</summary>
    True = 2
}

/// <summary>
/// Core algebraic operators for ECL₃^Q based on Strong Kleene (K₃) semantics.
/// All operators correspond to Łukasiewicz/Kleene min/max operations on {0,1,2}.
/// See Paper II §3 for algebraic axiomatization.
/// </summary>
public static class Operators
{
    /// <summary>
    /// Negation: ¬T=F, ¬U=U, ¬F=T.
    /// Involution: ¬¬A = A for all A.
    /// </summary>
    public static TruthValue Not(TruthValue a) =>
        (TruthValue)(2 - (int)a);

    /// <summary>
    /// Conjunction: A ∧ B = min(A, B).
    /// Corresponds to greatest lower bound in the lattice F ≤ U ≤ T.
    /// </summary>
    public static TruthValue And(TruthValue a, TruthValue b) =>
        (TruthValue)Math.Min((int)a, (int)b);

    /// <summary>
    /// Disjunction: A ∨ B = max(A, B).
    /// Corresponds to least upper bound in the lattice F ≤ U ≤ T.
    /// </summary>
    public static TruthValue Or(TruthValue a, TruthValue b) =>
        (TruthValue)Math.Max((int)a, (int)b);

    /// <summary>
    /// Material implication: A → B = ¬A ∨ B = max(2-A, B).
    /// Note: unlike classical logic, U→U = U (not T).
    /// </summary>
    public static TruthValue Implies(TruthValue a, TruthValue b) =>
        Or(Not(a), b);

    /// <summary>
    /// Biconditional: A ↔ B = (A→B) ∧ (B→A).
    /// </summary>
    public static TruthValue Iff(TruthValue a, TruthValue b) =>
        And(Implies(a, b), Implies(b, a));

    /// <summary>
    /// Designated value check: A formula is "true enough" to assert when its value is T.
    /// In ECL₃^Q the designated value is T (not {T,U} as in some K₃ variants).
    /// U does not designate — ontologically indeterminate propositions are not assertable.
    /// </summary>
    public static bool IsDesignated(TruthValue v) => v == TruthValue.True;

    /// <summary>
    /// Checks if a formula value is classical (T or F), i.e., not in superposition.
    /// Equivalent to: the corresponding world is an Eigenzustand (W_eigen), not W_super.
    /// </summary>
    public static bool IsClassical(TruthValue v) => v != TruthValue.Undetermined;
}
