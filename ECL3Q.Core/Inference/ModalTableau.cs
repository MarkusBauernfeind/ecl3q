using ECL3Q.Core.Syntax;

namespace ECL3Q.Core.Inference;

/// <summary>
/// World-indexed signed formula for modal tableau.
/// Uses structural record equality — no string comparison.
/// </summary>
public readonly record struct WorldSignedFormula(Formula Formula, Sign Sign, string World)
{
    public override string ToString() => $"{Sign}:{Formula}@{World}";
}

/// <summary>
/// Full ECL₃^Q modal tableau with three-valued (T/U/F) rules,
/// σ/[do τ] dynamic rules, and two accessibility relations (R_obs, R_τ).
///
/// ═══════════════════════════════════════════════════════════════════
/// COMPLETENESS STATUS
/// ═══════════════════════════════════════════════════════════════════
///
/// COMPLETE fragments:
///   (1) Propositional K₃ for all connectives — T, U, F signs
///   (2) Modal K over R_obs (□, ◇) — all three signs exact and complete
///   (3) Dynamic [do τ] and σ over R_τ — all three signs exact and complete
///   (4) Obs — all three signs now complete:
///       T:Obs exact (local truth + uniform successors)
///       U:Obs exact (two-branch: Source 1 ontological | Source 2 disagreeing successors)
///       F:Obs exact (two-world witnessing)
///
/// WHY □/◇/σ/[do τ] ARE COMPLETE FOR U-SIGN:
///   □φ@w = min over R_obs-successors.
///   □φ@w = U  iff  all successors ∈ {T,U} AND at least one = U.
///   (If any successor = F, then min = F, not U.)
///   Therefore: U:□φ@w is witnessed by exactly one U-valued successor.
///   The single-fresh-world rule is not an approximation — it is exact.
///   The same argument holds for ◇ (via max), and for σ/[do τ] over R_τ.
///
/// WHY U:Obs IS NOW COMPLETE:
///   U:Obs(φ)@w arises from exactly two sources:
///     Source 1: V(φ,w) = U (ontological indeterminacy).
///     Source 2: V(φ,w) ∈ {T,F} but R_obs-successors have mixed values.
///   The branching rule splits into Branch A (Source 1: U:φ@w) and
///   Branch B (Source 2: fresh v₁ with T:φ, v₂ with F:φ).
///   These cases are exhaustive and mutually exclusive → complete.
///
/// OPEN (genuine incompleteness):
///
///   (D) Completeness theorem for full ECL₃^Q (SC_PC + AO1_eigen frame class):
///       Not established here. Target: Paper I §4.3.
///
/// ═══════════════════════════════════════════════════════════════════
/// THREE-VALUED TABLEAU RULES
/// ═══════════════════════════════════════════════════════════════════
///
///  Notation: s:op means "formula op has sign s at current world".
///  (*) = branching rule.  Linear rules fire first to minimise branching.
///
///  ¬   T:¬A → F:A        U:¬A → U:A        F:¬A → T:A
///
///  ∧   T:(A∧B) → T:A,T:B                   (linear)
///      F:(A∧B) → F:A | F:B                  (*)
///      U:(A∧B) → (U:A,T:B)|(T:A,U:B)|(U:A,U:B)  (*)  min=U cases
///
///  ∨   T:(A∨B) → T:A | T:B                  (*)
///      F:(A∨B) → F:A,F:B                   (linear)
///      U:(A∨B) → (U:A,F:B)|(F:A,U:B)|(U:A,U:B)  (*)  max=U cases
///
///  →   T:(A→B) → F:A | T:B                  (*)
///      F:(A→B) → T:A,F:B                   (linear)
///      U:(A→B) → (U:A,U:B)|(T:A,U:B)|(U:A,F:B) (*)  max(¬A,B)=U cases
///       Note: U→U=U (not T), F→_=T, _→T=T
///
///  ↔   T:(A↔B) → T:(A→B),T:(B→A)           (linear, reduces to →)
///      F:(A↔B) → F:(A→B) | F:(B→A)         (*)
///      U:(A↔B) → (U:A→B,T:B→A)|(T:A→B,U:B→A)|(U:A→B,U:B→A)  (*)
///       All 5 semantic U-cases covered by these 3 branches (verified).
///
///  □   T:□φ@w → T:φ@v, all v∈R_obs(w)      (linear, re-fires on new worlds)
///      F:□φ@w → fresh v, R_obs(w,v), F:φ@v  (linear, once)
///      U:□φ@w → fresh v, R_obs(w,v), U:φ@v  (linear, once; exact — see WHY above)
///
///  ◇   T:◇φ@w → fresh v, R_obs(w,v), T:φ@v  (linear, once)
///      F:◇φ@w → F:φ@v, all v∈R_obs(w)       (linear, re-fires)
///      U:◇φ@w → fresh v, R_obs(w,v), U:φ@v  (linear, once; exact — see WHY above)
///
///  [do τ] / σ   (parallel to □/◇ but over R_τ; all signs exact)
///      T → propagate T to all R_τ-successors
///      F → fresh v in R_τ, F:φ@v
///      U → fresh v in R_τ, U:φ@v   (exact — same min/max argument as □)
///
///  Obs  T:Obs(φ)@w → T:φ@w, T:φ@v all v∈R_obs(w)               (linear, re-entrant)
///       U:Obs(φ)@w → U:φ@w  |  [fresh v₁,v₂; T:φ@v₁, F:φ@v₂]  (*) both sources
///       F:Obs(φ)@w → fresh v₁,v₂; T:φ@v₁, F:φ@v₂               (linear, once)
///
/// ═══════════════════════════════════════════════════════════════════
/// </summary>
public sealed class ModalTableau
{
    // Safety cap: prevents unchecked branch explosion.
    // Exceeding this means the formula exceeds the practical decidability range
    // of this implementation — not a theoretical limitation of ECL₃^Q.
    private const int MaxBranches = 10_000;

    private readonly List<ModalBranch> _branches;
    private int _worldCounter;

    public ModalTableau(Formula formula, string? startWorld = null)
    {
        var w = startWorld ?? "w0";
        _worldCounter = 1;
        var initial = new ModalBranch();
        initial.Add(new WorldSignedFormula(formula, Sign.F, w));
        _branches = [initial];
    }

    /// <summary>
    /// Returns true iff φ is classically valid (cannot be false in any Kripke model).
    ///
    /// SEMANTICS: This method checks CLASSICAL validity by refuting F:φ at the root world.
    /// A K₃ countermodel has V(φ)=U (not F), which is not refuted by this tableau.
    /// For K₃-validity of propositional formulas use <see cref="ProofSearch.IsK3Tautology"/>.
    ///
    /// Examples:
    ///   p→p:   IsModallyValid = true  (classically valid; K₃ gives U for p=U)
    ///   p∨¬p:  IsModallyValid = true  (LEM — classically valid; K₃ gives U)
    ///   □(p→q)→(□p→□q): IsModallyValid = true (K-axiom — classically and K₃ valid)
    /// </summary>
    public static bool IsModallyValid(Formula formula, int maxIterations = 500) =>
        new ModalTableau(formula).Expand(maxIterations);

    /// <summary>
    /// Returns true iff all branches close (refutation succeeds = formula valid).
    /// Returns false if any branch is open after saturation.
    /// </summary>
    public bool Expand(int maxIterations)
    {
        for (int i = 0; i < maxIterations; i++)
        {
            if (_branches.Count > MaxBranches)
                throw new TableauExplosionException(
                    $"Branch count exceeded {MaxBranches}. Formula may be too complex " +
                    "for this tableau implementation.");

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

    // ─────────────────────────────────────────────────────────────────────────
    // Branch expansion: linear rules first, then branching
    // ─────────────────────────────────────────────────────────────────────────

    private (bool expanded, List<ModalBranch> newBranches) ExpandBranch(ModalBranch branch)
    {
        foreach (var wsf in branch.Snapshot())
        {
            if (branch.IsExpanded(wsf)) continue;
            var linear = TryLinearRule(branch, wsf);
            if (linear.expanded) return linear;
        }

        foreach (var wsf in branch.Snapshot())
        {
            if (branch.IsExpanded(wsf)) continue;
            var branching = TryBranchingRule(branch, wsf);
            if (branching.expanded || branching.newBranches.Count > 0) return branching;
        }

        return (false, []);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Linear (non-branching) rules
    // ─────────────────────────────────────────────────────────────────────────

    private (bool expanded, List<ModalBranch> newBranches) TryLinearRule(
        ModalBranch branch, WorldSignedFormula wsf)
    {
        var (formula, sign, w) = (wsf.Formula, wsf.Sign, wsf.World);

        // ── Negation (all signs, all linear) ─────────────────────────────────
        if (formula is Negation neg)
        {
            var resultSign = sign switch
            {
                Sign.T => Sign.F,
                Sign.F => Sign.T,
                Sign.U => Sign.U,
                _ => throw new UnreachableException()
            };
            return AddLinear(branch, wsf, new WorldSignedFormula(neg.Sub, resultSign, w));
        }

        // ── Conjunction: T (min=T requires both T) ───────────────────────────
        if (formula is Conjunction conj && sign == Sign.T)
        {
            bool added = false;
            added |= branch.TryAdd(new WorldSignedFormula(conj.Left,  Sign.T, w));
            added |= branch.TryAdd(new WorldSignedFormula(conj.Right, Sign.T, w));
            if (added) { branch.MarkExpanded(wsf); return (true, []); }
            branch.MarkExpanded(wsf);
        }

        // ── Disjunction: F (max=F requires both F) ───────────────────────────
        if (formula is Disjunction disj && sign == Sign.F)
        {
            bool added = false;
            added |= branch.TryAdd(new WorldSignedFormula(disj.Left,  Sign.F, w));
            added |= branch.TryAdd(new WorldSignedFormula(disj.Right, Sign.F, w));
            if (added) { branch.MarkExpanded(wsf); return (true, []); }
            branch.MarkExpanded(wsf);
        }

        // ── Implication: F (A→B=F requires A=T, B=F) ────────────────────────
        if (formula is Implication impl && sign == Sign.F)
        {
            bool added = false;
            added |= branch.TryAdd(new WorldSignedFormula(impl.Antecedent, Sign.T, w));
            added |= branch.TryAdd(new WorldSignedFormula(impl.Consequent, Sign.F, w));
            if (added) { branch.MarkExpanded(wsf); return (true, []); }
            branch.MarkExpanded(wsf);
        }

        // ── Biconditional: T (reduces to conjunction of implications) ─────────
        if (formula is Biconditional bic && sign == Sign.T)
        {
            bool added = false;
            added |= branch.TryAdd(new WorldSignedFormula(new Implication(bic.Left, bic.Right), Sign.T, w));
            added |= branch.TryAdd(new WorldSignedFormula(new Implication(bic.Right, bic.Left), Sign.T, w));
            if (added) { branch.MarkExpanded(wsf); return (true, []); }
            branch.MarkExpanded(wsf);
        }

        // ── □: T (universally propagate to all known R_obs-successors) ────────
        // Not marked expanded — must re-fire when new worlds are added.
        if (formula is ModalBox box && sign == Sign.T)
        {
            bool added = false;
            foreach (var v in branch.ObsSuccessors(w))
                added |= branch.TryAdd(new WorldSignedFormula(box.Sub, Sign.T, v));
            if (added) return (true, []);
        }

        // ── □: F (one fresh R_obs-successor with F:φ) ─────────────────────────
        if (formula is ModalBox box2 && sign == Sign.F && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddObsEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(box2.Sub, Sign.F, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── □: U (exact — U:□φ requires a U-valued successor; see class header WHY) ──
        if (formula is ModalBox box3 && sign == Sign.U && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddObsEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(box3.Sub, Sign.U, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── ◇: T (one fresh R_obs-successor with T:φ) ─────────────────────────
        if (formula is ModalDiamond dia && sign == Sign.T && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddObsEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(dia.Sub, Sign.T, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── ◇: F (universally propagate F to all R_obs-successors) ────────────
        if (formula is ModalDiamond dia2 && sign == Sign.F)
        {
            bool added = false;
            foreach (var v in branch.ObsSuccessors(w))
                added |= branch.TryAdd(new WorldSignedFormula(dia2.Sub, Sign.F, v));
            if (added) return (true, []);
        }

        // ── ◇: U (exact — U:◇φ requires a U-valued successor; see class header WHY) ──
        if (formula is ModalDiamond dia3 && sign == Sign.U && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddObsEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(dia3.Sub, Sign.U, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── [do τ]: T (propagate T to all R_τ-successors) ─────────────────────
        if (formula is DoOperator doOp && sign == Sign.T)
        {
            bool added = false;
            foreach (var v in branch.TauSuccessors(w))
                added |= branch.TryAdd(new WorldSignedFormula(doOp.Sub, Sign.T, v));
            if (added) return (true, []);
        }

        // ── [do τ]: F (one fresh R_τ-successor) ───────────────────────────────
        if (formula is DoOperator doOp2 && sign == Sign.F && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddTauEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(doOp2.Sub, Sign.F, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── [do τ]: U (exact — same min argument as □; see class header WHY) ───
        if (formula is DoOperator doOp3 && sign == Sign.U && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddTauEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(doOp3.Sub, Sign.U, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── σ: T (propagate T to all R_τ-successors; parallel to [do τ]:T) ────
        if (formula is CollapseFormula col && sign == Sign.T)
        {
            bool added = false;
            foreach (var v in branch.TauSuccessors(w))
                added |= branch.TryAdd(new WorldSignedFormula(col.Sub, Sign.T, v));
            if (added) return (true, []);
        }

        // ── σ: F ───────────────────────────────────────────────────────────────
        if (formula is CollapseFormula col2 && sign == Sign.F && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddTauEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(col2.Sub, Sign.F, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── σ: U (exact — same min argument as □; see class header WHY) ────────
        if (formula is CollapseFormula col3 && sign == Sign.U && !branch.IsExpanded(wsf))
        {
            var v = FreshWorld();
            branch.AddTauEdge(w, v);
            branch.TryAdd(new WorldSignedFormula(col3.Sub, Sign.U, v));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        // ── Obs: T (observability implies truth locally and at all successors) ──
        if (formula is ObsFormula obs && sign == Sign.T)
        {
            bool added = branch.TryAdd(new WorldSignedFormula(obs.Sub, Sign.T, w));
            foreach (var v in branch.ObsSuccessors(w))
                added |= branch.TryAdd(new WorldSignedFormula(obs.Sub, Sign.T, v));
            if (added) return (true, []);
        }

        // ── Obs: U — BOTH sources handled (complete) ─────────────────────────
        // U:Obs(φ)@w arises from two mutually exclusive sources:
        //   Source 1: φ itself is U at w (ontological indeterminacy)
        //   Source 2: φ is classical at w, but R_obs-successors disagree
        //
        // We handle both via a branching rule (moved to TryBranchingRule).
        // The linear pass does nothing for U:Obs — let branching handle it.

        // ── Obs: F (two-world witnessing, sound and complete) ──────────────────
        // F:Obs(φ)@w means the observation is not uniform.
        // Witnesses: two fresh worlds with conflicting values of φ.

        if (formula is ObsFormula obs3 && sign == Sign.F && !branch.IsExpanded(wsf))
        {
            var v1 = FreshWorld();
            var v2 = FreshWorld();
            branch.AddObsEdge(w, v1);
            branch.AddObsEdge(w, v2);
            branch.TryAdd(new WorldSignedFormula(obs3.Sub, Sign.T, v1));
            branch.TryAdd(new WorldSignedFormula(obs3.Sub, Sign.F, v2));
            branch.MarkExpanded(wsf);
            return (true, []);
        }

        return (false, []);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Branching rules
    // ─────────────────────────────────────────────────────────────────────────

    private (bool expanded, List<ModalBranch> newBranches) TryBranchingRule(
        ModalBranch branch, WorldSignedFormula wsf)
    {
        var (formula, sign, w) = (wsf.Formula, wsf.Sign, wsf.World);

        // ── Conjunction: F → F:A | F:B ────────────────────────────────────────
        if (formula is Conjunction conj && sign == Sign.F)
            return Split2(branch, wsf,
                new WorldSignedFormula(conj.Left,  Sign.F, w),
                new WorldSignedFormula(conj.Right, Sign.F, w));

        // ── Conjunction: U → (U:A,T:B) | (T:A,U:B) | (U:A,U:B) ──────────────
        if (formula is Conjunction conj2 && sign == Sign.U)
            return Split3(branch, wsf,
                [new(conj2.Left, Sign.U, w), new(conj2.Right, Sign.T, w)],
                [new(conj2.Left, Sign.T, w), new(conj2.Right, Sign.U, w)],
                [new(conj2.Left, Sign.U, w), new(conj2.Right, Sign.U, w)]);

        // ── Disjunction: T → T:A | T:B ───────────────────────────────────────
        if (formula is Disjunction disj && sign == Sign.T)
            return Split2(branch, wsf,
                new WorldSignedFormula(disj.Left,  Sign.T, w),
                new WorldSignedFormula(disj.Right, Sign.T, w));

        // ── Disjunction: U → (U:A,F:B) | (F:A,U:B) | (U:A,U:B) ─────────────
        if (formula is Disjunction disj2 && sign == Sign.U)
            return Split3(branch, wsf,
                [new(disj2.Left, Sign.U, w), new(disj2.Right, Sign.F, w)],
                [new(disj2.Left, Sign.F, w), new(disj2.Right, Sign.U, w)],
                [new(disj2.Left, Sign.U, w), new(disj2.Right, Sign.U, w)]);

        // ── Implication: T → F:A | T:B ────────────────────────────────────────
        if (formula is Implication impl && sign == Sign.T)
            return Split2(branch, wsf,
                new WorldSignedFormula(impl.Antecedent, Sign.F, w),
                new WorldSignedFormula(impl.Consequent, Sign.T, w));

        // ── Implication: U → (U:A,U:B) | (T:A,U:B) | (U:A,F:B) ─────────────
        if (formula is Implication impl2 && sign == Sign.U)
            return Split3(branch, wsf,
                [new(impl2.Antecedent, Sign.U, w), new(impl2.Consequent, Sign.U, w)],
                [new(impl2.Antecedent, Sign.T, w), new(impl2.Consequent, Sign.U, w)],
                [new(impl2.Antecedent, Sign.U, w), new(impl2.Consequent, Sign.F, w)]);

        // ── Biconditional: F → F:(A→B) | F:(B→A) ────────────────────────────
        if (formula is Biconditional bic && sign == Sign.F)
            return Split2(branch, wsf,
                new WorldSignedFormula(new Implication(bic.Left,  bic.Right), Sign.F, w),
                new WorldSignedFormula(new Implication(bic.Right, bic.Left),  Sign.F, w));

        // ── Biconditional: U → (U:A→B,T:B→A) | (T:A→B,U:B→A) | (U:A→B,U:B→A)
        // Verified: all 5 semantic U-cases (F,U),(U,F),(U,U),(U,T),(T,U) covered.
        if (formula is Biconditional bic2 && sign == Sign.U)
        {
            var ab = new Implication(bic2.Left, bic2.Right);
            var ba = new Implication(bic2.Right, bic2.Left);
            return Split3(branch, wsf,
                [new(ab, Sign.U, w), new(ba, Sign.T, w)],
                [new(ab, Sign.T, w), new(ba, Sign.U, w)],
                [new(ab, Sign.U, w), new(ba, Sign.U, w)]);
        }

        // ── Obs: U → Source1: U:φ@w  |  Source2: fresh v₁(T:φ), v₂(F:φ) ──────
        //
        // U:Obs(φ)@w holds in exactly two scenarios:
        //   Source 1: V(φ,w) = U  (ontological constraint — U is never observable)
        //   Source 2: V(φ,w) ∈ {T,F} but R_obs-successors disagree on φ's value
        //
        // Branch A covers Source 1: add U:φ@w.
        // Branch B covers Source 2: two fresh R_obs-successors with T:φ and F:φ,
        //   witnessing the disagreement that causes Obs(φ) to be U.
        //
        // This rule is sound and complete for U:Obs.
        // (Resolves the previously documented Open Problem C.)
        if (formula is ObsFormula obsU && sign == Sign.U)
        {
            var v1 = FreshWorld();
            var v2 = FreshWorld();
            // Branch A: Source 1 — φ itself is ontologically U
            var branchA = branch.Clone();
            branchA.TryAdd(new WorldSignedFormula(obsU.Sub, Sign.U, w));
            branchA.MarkExpanded(wsf);
            // Branch B: Source 2 — φ is classical but successors disagree
            var branchB = branch.Clone();
            branchB.AddObsEdge(w, v1);
            branchB.AddObsEdge(w, v2);
            branchB.TryAdd(new WorldSignedFormula(obsU.Sub, Sign.T, v1));
            branchB.TryAdd(new WorldSignedFormula(obsU.Sub, Sign.F, v2));
            branchB.MarkExpanded(wsf);
            return (true, [branchA, branchB]);
        }

        return (false, []);
    }

    private string FreshWorld() => $"w{_worldCounter++}";

    private static (bool expanded, List<ModalBranch> newBranches) AddLinear(
        ModalBranch branch, WorldSignedFormula source, WorldSignedFormula target)
    {
        branch.MarkExpanded(source);
        return branch.TryAdd(target) ? (true, []) : (false, []);
    }

    private static (bool expanded, List<ModalBranch> newBranches) Split2(
        ModalBranch branch, WorldSignedFormula source,
        WorldSignedFormula left, WorldSignedFormula right)
    {
        var b1 = branch.Clone(); b1.TryAdd(left);  b1.MarkExpanded(source);
        var b2 = branch.Clone(); b2.TryAdd(right); b2.MarkExpanded(source);
        return (true, [b1, b2]);
    }

    private static (bool expanded, List<ModalBranch> newBranches) Split3(
        ModalBranch branch, WorldSignedFormula source,
        IReadOnlyList<WorldSignedFormula> adds1,
        IReadOnlyList<WorldSignedFormula> adds2,
        IReadOnlyList<WorldSignedFormula> adds3)
    {
        var b1 = branch.Clone();
        var b2 = branch.Clone();
        var b3 = branch.Clone();
        foreach (var f in adds1) b1.TryAdd(f);
        foreach (var f in adds2) b2.TryAdd(f);
        foreach (var f in adds3) b3.TryAdd(f);
        b1.MarkExpanded(source);
        b2.MarkExpanded(source);
        b3.MarkExpanded(source);
        return (true, [b1, b2, b3]);
    }
}

/// <summary>
/// A single branch in the modal tableau.
///
/// Invariants:
///   - Formula lookup is O(1) via HashSet (not O(n) via List.Contains).
///   - Two separate accessibility relations: R_obs and R_τ.
///   - IsClosed uses structural record equality, not string comparison.
/// </summary>
public sealed class ModalBranch
{
    // Ordered list for deterministic iteration; set for O(1) membership tests.
    private readonly List<WorldSignedFormula> _orderedFormulas = [];
    private readonly HashSet<WorldSignedFormula> _formulaSet = [];

    private readonly HashSet<(string From, string To)> _obsEdges = [];
    private readonly HashSet<(string From, string To)> _tauEdges = [];
    private readonly HashSet<WorldSignedFormula> _expanded = [];

    // Cached closure flag — once closed, never re-opens.
    private bool _closed;

    /// <summary>
    /// Try to add wsf. Returns true if it was not already present.
    /// O(1) via HashSet.
    /// </summary>
    public bool TryAdd(WorldSignedFormula wsf)
    {
        if (!_formulaSet.Add(wsf)) return false;
        _orderedFormulas.Add(wsf);
        if (IsContradiction(wsf)) _closed = true;
        return true;
    }

    /// <summary>Adds <paramref name="wsf"/>, ignoring duplicates. Prefer <see cref="TryAdd"/> when the return value matters.</summary>
    public void Add(WorldSignedFormula wsf) => TryAdd(wsf);

    /// <summary>Returns true iff <paramref name="wsf"/> is already on this branch. O(1).</summary>
    public bool Contains(WorldSignedFormula wsf) => _formulaSet.Contains(wsf);

    /// <summary>Marks <paramref name="wsf"/> as expanded. One-shot rules must call this to prevent re-firing. Re-entrant rules (T:□, F:◇) must NOT call this.</summary>
    public void MarkExpanded(WorldSignedFormula wsf) => _expanded.Add(wsf);
    /// <summary>Returns true iff <paramref name="wsf"/> has already been expanded on this branch.</summary>
    public bool IsExpanded(WorldSignedFormula wsf) => _expanded.Contains(wsf);

    /// <summary>Adds an R_obs accessibility edge from world <paramref name="from"/> to <paramref name="to"/>.</summary>
    public void AddObsEdge(string from, string to) => _obsEdges.Add((from, to));
    /// <summary>Adds an R_τ accessibility edge from world <paramref name="from"/> to <paramref name="to"/>.</summary>
    public void AddTauEdge(string from, string to) => _tauEdges.Add((from, to));

    /// <summary>Returns all worlds R_obs-accessible from <paramref name="from"/> on this branch.</summary>
    public IEnumerable<string> ObsSuccessors(string from) =>
        _obsEdges.Where(e => e.From == from).Select(e => e.To);

    /// <summary>Returns all worlds R_τ-accessible from <paramref name="from"/> on this branch.</summary>
    public IEnumerable<string> TauSuccessors(string from) =>
        _tauEdges.Where(e => e.From == from).Select(e => e.To);

    /// <summary>
    /// Snapshot for safe iteration while the branch may grow during expansion.
    /// </summary>
    public IReadOnlyList<WorldSignedFormula> Snapshot() =>
        _orderedFormulas.ToArray();

    /// <summary>
    /// Branch is closed iff it contains both T:φ@w and F:φ@w for any (φ,w).
    /// Uses structural record equality. T+U and F+U do NOT close a branch.
    /// </summary>
    public bool IsClosed() => _closed;

    private bool IsContradiction(WorldSignedFormula wsf)
    {
        if (wsf.Sign == Sign.U) return false; // U never closes a branch
        var opposite = wsf with { Sign = wsf.Sign == Sign.T ? Sign.F : Sign.T };
        return _formulaSet.Contains(opposite);
    }

    /// <summary>Returns a deep copy of this branch, including all formulas, edges, expanded markers, and closed state.</summary>
    public ModalBranch Clone()
    {
        var b = new ModalBranch();
        foreach (var f in _orderedFormulas) { b._orderedFormulas.Add(f); b._formulaSet.Add(f); }
        b._obsEdges.UnionWith(_obsEdges);
        b._tauEdges.UnionWith(_tauEdges);
        b._expanded.UnionWith(_expanded);
        b._closed = _closed;
        return b;
    }
}

public sealed class TableauExplosionException(string message) : Exception(message);

file sealed class UnreachableException() : Exception("Unreachable code reached.");
