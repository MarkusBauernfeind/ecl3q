using ECL3Q.Core.Algebra;

namespace ECL3Q.Core.Semantics;

/// <summary>
/// σ-Collapse: transition from W_super to W_eigen.
///
/// The σ-operator models the collapse of ontological indeterminacy,
/// analogous to quantum measurement. Key properties:
///
/// - σ maps W_super worlds to W_eigen worlds via R_τ relation
/// - After σ-collapse, all action variables have classical values {T, F}
/// - σ-collapse is irreversible: W_eigen worlds have no W_super successors
///
/// See Paper III (QM-Erweiterung), SR31–SR71 for formal development.
/// </summary>
public static class CollapseOperator
{
    /// <summary>
    /// Performs σ-collapse: given a W_super world and a target W_eigen world,
    /// returns the truth value of formula φ in the post-collapse state.
    ///
    /// The collapse is non-deterministic in general (multiple R_τ targets possible).
    /// For deterministic scenarios, the target world must be specified.
    /// </summary>
    public static TruthValue Collapse(
        Model model,
        World superWorld,
        World eigenWorld,
        Syntax.Formula formula)
    {
        // Verify pre-condition: superWorld should be W_super
        if (model.GetWorldType(superWorld) != WorldType.Super)
            throw new ArgumentException(
                $"σ-collapse pre-condition violated: {superWorld} is not W_super. " +
                "Collapse is only meaningful for superposition worlds.");

        // Verify collapse relation holds
        if (!model.Frame.CollapseSuccessors(superWorld).Contains(eigenWorld))
            throw new ArgumentException(
                $"σ-collapse relation violated: {eigenWorld} is not a R_τ successor of {superWorld}.");

        // Post-collapse evaluation: φ at the eigenstate
        return model.Evaluate(formula, eigenWorld);
    }

    /// <summary>
    /// Non-deterministic collapse: returns all possible post-collapse values.
    /// Corresponds to the set of R_τ successors and their valuations.
    /// </summary>
    public static IEnumerable<(World EigenWorld, TruthValue Value)> CollapseAll(
        Model model,
        World superWorld,
        Syntax.Formula formula)
    {
        foreach (var target in model.Frame.CollapseSuccessors(superWorld))
        {
            yield return (target, model.Evaluate(formula, target));
        }
    }

    /// <summary>
    /// Checks AO1_eigen (Axiom schema):
    /// After collapse, no ontological indeterminacy remains for action variables.
    /// Formally: σ(τ) ∈ {T, F} for all action variables τ.
    ///
    /// This is required for completeness (together with AO1_eigen as axiom schema).
    /// Note: SC_PC is a theorem under Option B, not a separate frame condition.
    /// </summary>
    public static bool VerifyAO1Eigen(Model model, World superWorld, string actionVar)
    {
        var formula = new Syntax.Atom(actionVar);
        return model.Frame.CollapseSuccessors(superWorld)
            .All(eigenWorld =>
            {
                var val = model.Evaluate(formula, eigenWorld);
                return val != TruthValue.Undetermined;
            });
    }
}
