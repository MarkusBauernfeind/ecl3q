namespace ECL3Q.Core.Syntax;

/// <summary>
/// Static factory methods for concise formula construction.
/// Mirrors the formal syntax of ECL₃^Q (Paper I §2).
///
/// Usage: <c>using static ECL3Q.Core.Syntax.F;</c>
///
/// Example:
/// <code>
/// using static ECL3Q.Core.Syntax.F;
/// var phi = Implies(And(P, Q), Obs(P));
/// </code>
/// </summary>
public static class F
{
    /// <summary>Propositional atom with the given name.</summary>
    public static Atom Atom(string name) => new(name);

    /// <summary>Negation: ¬φ</summary>
    public static Negation Not(Formula f) => new(f);

    /// <summary>Conjunction: φ ∧ ψ  (Strong Kleene min)</summary>
    public static Conjunction And(Formula a, Formula b) => new(a, b);

    /// <summary>Disjunction: φ ∨ ψ  (Strong Kleene max)</summary>
    public static Disjunction Or(Formula a, Formula b) => new(a, b);

    /// <summary>Material implication: φ → ψ  (max(¬φ, ψ))</summary>
    public static Implication Implies(Formula a, Formula b) => new(a, b);

    /// <summary>Biconditional: φ ↔ ψ  ((φ→ψ) ∧ (ψ→φ))</summary>
    public static Biconditional Iff(Formula a, Formula b) => new(a, b);

    /// <summary>Observability formula: Obs(φ) — "φ is observable at the current world".</summary>
    public static ObsFormula Obs(Formula f) => new(f);

    /// <summary>Modal necessity: □φ — "φ holds at all R_obs-accessible worlds".</summary>
    public static ModalBox Box(Formula f) => new(f);

    /// <summary>Modal possibility: ◇φ — "φ holds at some R_obs-accessible world".</summary>
    public static ModalDiamond Diamond(Formula f) => new(f);

    /// <summary>Dynamic operator: [do action]φ — "φ holds after performing action via R_τ".</summary>
    public static DoOperator Do(string action, Formula f) => new(action, f);

    /// <summary>σ-Collapse: σ(φ) — "φ holds after σ-collapse from W_super to W_eigen via R_τ".</summary>
    public static CollapseFormula Collapse(Formula f) => new(f);

    /// <summary>Convenience atom <c>p</c>.</summary>
    public static Atom P => Atom("p");

    /// <summary>Convenience atom <c>q</c>.</summary>
    public static Atom Q => Atom("q");

    /// <summary>Convenience atom <c>r</c>.</summary>
    public static Atom R => Atom("r");
}
