namespace ECL3Q.Core.Algebra;

/// <summary>
/// The Observability Operator Obs: F₃ → F₃.
///
/// THEOREM A4 (Paper II): Obs is NOT F₃-linear.
/// There is no matrix M ∈ Mat(F₃) such that Obs(v) = M·v for all valuation vectors v.
/// Consequently, Obs cannot be represented as a tensor/matrix operation.
///
/// OPEN PROBLEM F4 (Paper II): Whether Obs admits any linear representation
/// in a larger algebraic structure remains open. Do NOT implement as matrix.
///
/// OC-LEMMA STATUS: The naive "obs_w(A∧B) = min(obs_w(A), obs_w(B))" is FALSIFIED
/// via explicit countermodel (see SR_basis). Only OC-weak holds:
///   obs_w(A∧B) ≤ min(obs_w(A), obs_w(B)) for all models.
///
/// Obs is therefore implemented as an abstract function over models, not algebraically.
/// Concrete behavior is determined by the Frame + Valuation (see ECL3Q.Semantics).
/// </summary>
public static class ObsOperator
{
    /// <summary>
    /// Compute observability of a truth value in a given observational context.
    ///
    /// The obs_w value depends on:
    ///   (a) the truth value of A at world w
    ///   (b) the observation relation R_obs from w
    ///   (c) the specific model structure
    ///
    /// Constraints (from Paper II §4):
    ///   - If v(A,w) = U  → obs_w(A) = U  (ontologically indeterminate ⟹ not directly observable)
    ///   - If v(A,w) = T  → obs_w(A) ∈ {T, U}  (may or may not be observable)
    ///   - If v(A,w) = F  → obs_w(A) ∈ {F, U}  (falsity may or may not be observable)
    ///
    /// OC-weak (proven): obs_w(A∧B) ≤ min(obs_w(A), obs_w(B))
    /// OC (falsified):   obs_w(A∧B) = min(obs_w(A), obs_w(B))  — DOES NOT HOLD in general
    /// </summary>
    /// <param name="formulaValue">The truth value of the formula at world w.</param>
    /// <param name="observabilityContext">
    /// A function encoding the observation relation for the current world.
    /// Returns the observed truth value given a formula's base truth value.
    /// </param>
    public static TruthValue Evaluate(
        TruthValue formulaValue,
        Func<TruthValue, TruthValue> observabilityContext)
    {
        // Constraint: U is never directly observable (ontological, not epistemic)
        if (formulaValue == TruthValue.Undetermined)
            return TruthValue.Undetermined;

        var observed = observabilityContext(formulaValue);

        // Validate output constraints
        if (formulaValue == TruthValue.True && observed == TruthValue.False)
            throw new InvalidOperationException(
                "Obs invariant violated: obs_w(T) cannot be F. " +
                "A true proposition cannot be observed as false.");

        if (formulaValue == TruthValue.False && observed == TruthValue.True)
            throw new InvalidOperationException(
                "Obs invariant violated: obs_w(F) cannot be T. " +
                "A false proposition cannot be observed as true.");

        return observed;
    }

    /// <summary>
    /// Verifies OC-weak for a given conjunction in a model context.
    /// obs_w(A∧B) ≤ min(obs_w(A), obs_w(B))
    ///
    /// This SHOULD hold for all well-formed models. Violation indicates model error.
    /// </summary>
    public static bool VerifyOcWeak(
        TruthValue obsAandB,
        TruthValue obsA,
        TruthValue obsB)
    {
        var minObs = (TruthValue)Math.Min((int)obsA, (int)obsB);
        return (int)obsAandB <= (int)minObs;
    }

    /// <summary>
    /// Demonstrates that OC (strong) does not hold in general for ECL₃^Q.
    ///
    /// IMPORTANT: The standard <see cref="Model.EvaluateObs"/> implementation
    /// uses a value-based Obs semantics (uniform agreement across R_obs-successors)
    /// under which OC is in fact valid. The OC falsification from SR_basis applies
    /// to alternative Obs semantics where joint observability of A∧B cannot be
    /// reduced to the individual observabilities of A and B — precisely because
    /// Obs is not F₃-linear (Theorem A4, Paper II).
    ///
    /// This method demonstrates the *algebraic* impossibility: it returns values
    /// that satisfy OC-weak but not OC-strong, showing the logical possibility
    /// of OC failure. A full countermodel requires an Obs function that implements
    /// joint-witnessing constraints beyond the standard value-based semantics.
    ///
    /// Returns (obs_A∧B, obs_A, obs_B) = (U, T, T).
    /// Interpretation: A and B are individually observable, but their conjunction
    /// is not (requires joint witnessing which is not guaranteed).
    /// OC says this should be min(T,T)=T — but the returned value is U.
    /// OC-weak: U ≤ T. ✓
    /// </summary>
    public static (TruthValue obsConjunction, TruthValue obsA, TruthValue obsB)
        GetOcCountermodelValues()
    {
        // Values asserted from the formal countermodel in SR_basis.
        // These cannot be reproduced by the standard EvaluateObs —
        // that implementation satisfies OC by construction (see class header).
        // A non-standard Obs function (joint-witnessing semantics) is required.
        return (TruthValue.Undetermined, TruthValue.True, TruthValue.True);
    }
}
