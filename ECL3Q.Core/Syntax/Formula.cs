namespace ECL3Q.Core.Syntax;

/// <summary>
/// Abstract base for all ECL₃^Q formulas.
/// Formulas are immutable value-semantic records.
/// See Paper I §2 for the syntax definition.
/// </summary>
public abstract record Formula
{
    /// <summary>Returns all immediate subformulas (direct children in the AST).</summary>
    public abstract IEnumerable<Formula> Subformulas();

    /// <summary>
    /// Returns all proper subformulas (transitive closure of <see cref="Subformulas"/>).
    /// Does not include the formula itself.
    /// </summary>
    public IEnumerable<Formula> AllSubformulas()
    {
        foreach (var sub in Subformulas())
        {
            yield return sub;
            foreach (var subsub in sub.AllSubformulas())
                yield return subsub;
        }
    }

    /// <summary>
    /// Returns all distinct atomic proposition names occurring in this formula.
    /// </summary>
    public IEnumerable<string> Atoms() =>
        AllSubformulas()
            .OfType<Atom>()
            .Select(a => a.Name)
            .Distinct();

    /// <summary>
    /// Depth of the formula tree. Atoms have depth 0;
    /// each connective adds 1 to the maximum depth of its subformulas.
    /// </summary>
    public abstract int Depth();
}

// ─── Atomic Formula ───────────────────────────────────────────────────────────

/// <summary>
/// Propositional atom: p, q, r, ...
/// Also used for action variables τ in [do τ] operator.
/// </summary>
public sealed record Atom(string Name) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [];
    public override int Depth() => 0;
    public override string ToString() => Name;
}

// ─── Classical Connectives ────────────────────────────────────────────────────

/// <summary>Negation: ¬A</summary>
public sealed record Negation(Formula Sub) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Sub];
    public override int Depth() => 1 + Sub.Depth();
    public override string ToString() => $"¬{Sub}";
}

/// <summary>Conjunction: A ∧ B</summary>
public sealed record Conjunction(Formula Left, Formula Right) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Left, Right];
    public override int Depth() => 1 + Math.Max(Left.Depth(), Right.Depth());
    public override string ToString() => $"({Left} ∧ {Right})";
}

/// <summary>Disjunction: A ∨ B</summary>
public sealed record Disjunction(Formula Left, Formula Right) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Left, Right];
    public override int Depth() => 1 + Math.Max(Left.Depth(), Right.Depth());
    public override string ToString() => $"({Left} ∨ {Right})";
}

/// <summary>Material implication: A → B</summary>
public sealed record Implication(Formula Antecedent, Formula Consequent) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Antecedent, Consequent];
    public override int Depth() => 1 + Math.Max(Antecedent.Depth(), Consequent.Depth());
    public override string ToString() => $"({Antecedent} → {Consequent})";
}

/// <summary>Biconditional: A ↔ B</summary>
public sealed record Biconditional(Formula Left, Formula Right) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Left, Right];
    public override int Depth() => 1 + Math.Max(Left.Depth(), Right.Depth());
    public override string ToString() => $"({Left} ↔ {Right})";
}

// ─── ECL₃^Q-Specific Operators ───────────────────────────────────────────────

/// <summary>
/// Observability formula: Obs(A)
/// "A is observable at the current world."
/// Evaluated via obs_w function (not algebraically — F4 is open).
/// See Paper I §3.2, Paper II Theorem A4.
/// </summary>
public sealed record ObsFormula(Formula Sub) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Sub];
    public override int Depth() => 1 + Sub.Depth();
    public override string ToString() => $"Obs({Sub})";
}

/// <summary>
/// Modal necessity: □A
/// "A holds in all worlds accessible via R_obs."
/// </summary>
public sealed record ModalBox(Formula Sub) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Sub];
    public override int Depth() => 1 + Sub.Depth();
    public override string ToString() => $"□{Sub}";
}

/// <summary>
/// Modal possibility: ◇A
/// "A holds in some world accessible via R_obs."
/// </summary>
public sealed record ModalDiamond(Formula Sub) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Sub];
    public override int Depth() => 1 + Sub.Depth();
    public override string ToString() => $"◇{Sub}";
}

/// <summary>
/// Dynamic operator: [do τ]A
/// "After performing action τ, A holds."
/// τ is the collapse-inducing action (σ-event).
/// See Paper IV for τ-Framework.
/// </summary>
public sealed record DoOperator(string Action, Formula Sub) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Sub];
    public override int Depth() => 1 + Sub.Depth();
    public override string ToString() => $"[do {Action}]{Sub}";
}

/// <summary>
/// Collapse formula: σ(A)
/// "A after σ-collapse (transition from W_super to W_eigen)."
/// Core operator of the τ-Framework extension.
/// Option B: W_super/W_eigen are semantically defined via V(τ,·), not primitive.
/// </summary>
public sealed record CollapseFormula(Formula Sub) : Formula
{
    public override IEnumerable<Formula> Subformulas() => [Sub];
    public override int Depth() => 1 + Sub.Depth();
    public override string ToString() => $"σ({Sub})";
}
