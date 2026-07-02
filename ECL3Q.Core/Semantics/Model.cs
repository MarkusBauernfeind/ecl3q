using ECL3Q.Core.Algebra;
using ECL3Q.Core.Syntax;

namespace ECL3Q.Core.Semantics;

/// <summary>
/// Valuation V: Atom × World → TruthValue.
/// Assigns truth values to atomic propositions at each world.
///
/// OPTION B: World type (W_super vs W_eigen) is determined from valuation:
///   w ∈ W_super iff ∃ action variable τ: V(τ, w) = U
///   w ∈ W_eigen iff ∀ action variables τ: V(τ, w) ∈ {T, F}
///
/// Action variables are atoms prefixed with "τ" or registered via ActionVariables.
/// </summary>
public class Valuation
{
    private readonly Dictionary<(string Atom, World World), TruthValue> _map;
    private readonly HashSet<string> _actionVariables;

    public Valuation(
        IEnumerable<((string Atom, World World), TruthValue Value)> assignments,
        IEnumerable<string>? actionVariables = null)
    {
        _map = assignments.ToDictionary(a => a.Item1, a => a.Value);
        _actionVariables = actionVariables?.ToHashSet() ?? [];
    }

    /// <summary>Get V(atom, world). Returns F if undefined (closed-world assumption).</summary>
    public TruthValue Get(string atom, World world) =>
        _map.TryGetValue((atom, world), out var v) ? v : TruthValue.False;

    /// <summary>
    /// Determines WorldType for w under Option B semantics.
    /// w ∈ W_super iff any action variable has value U at w.
    /// </summary>
    public WorldType GetWorldType(World w) =>
        _actionVariables.Any(τ => Get(τ, w) == TruthValue.Undetermined)
            ? WorldType.Super
            : WorldType.Eigen;

    /// <summary>The set of atom names designated as action variables (used to determine world type under Option B).</summary>
    public IReadOnlySet<string> ActionVariables => _actionVariables;
}

/// <summary>
/// A Kripke model M = (F, V) for ECL₃^Q.
/// Provides formula evaluation at worlds.
/// See Paper I §3 for full semantics.
/// </summary>
public class Model
{
    /// <summary>The Kripke frame (worlds and accessibility relations) underlying this model.</summary>
    public Frame Frame { get; }
    /// <summary>The valuation function V: Atom × World → TruthValue.</summary>
    public Valuation Valuation { get; }

    public Model(Frame frame, Valuation valuation)
    {
        Frame = frame;
        Valuation = valuation;
    }

    /// <summary>
    /// Evaluates a formula at a world: V(φ, w).
    /// Core semantic function implementing Paper I §3 clauses.
    /// </summary>
    public TruthValue Evaluate(Formula formula, World world) => formula switch
    {
        Atom a => Valuation.Get(a.Name, world),

        Negation n => Operators.Not(Evaluate(n.Sub, world)),

        Conjunction c => Operators.And(Evaluate(c.Left, world), Evaluate(c.Right, world)),

        Disjunction d => Operators.Or(Evaluate(d.Left, world), Evaluate(d.Right, world)),

        Implication i => Operators.Implies(Evaluate(i.Antecedent, world), Evaluate(i.Consequent, world)),

        Biconditional b => Operators.Iff(Evaluate(b.Left, world), Evaluate(b.Right, world)),

        // □A: min over all R_obs-successors (necessity = greatest lower bound)
        ModalBox box => Frame.ObsSuccessors(world)
            .Select(v => Evaluate(box.Sub, v))
            .DefaultIfEmpty(TruthValue.True)  // vacuously true if no successors
            .Aggregate(TruthValue.True, Operators.And),

        // ◇A: max over all R_obs-successors (possibility = least upper bound)
        ModalDiamond diamond => Frame.ObsSuccessors(world)
            .Select(v => Evaluate(diamond.Sub, v))
            .DefaultIfEmpty(TruthValue.False)  // vacuously false if no successors
            .Aggregate(TruthValue.False, Operators.Or),

        // Obs(A): observability at w — depends on observation relation
        // Uses model-level obs function (not algebraic — F4 open, Theorem A4)
        ObsFormula obs => EvaluateObs(obs.Sub, world),

        // σ(A): value of A after collapse from w via R_τ.
        // Semantics: min over R_τ-successors (parallel to □ over R_obs).
        // If w has no R_τ-successors, SC_PC is violated — U signals the error.
        CollapseFormula collapse => Frame.CollapseSuccessors(world)
            .Select(v => Evaluate(collapse.Sub, v))
            .DefaultIfEmpty(TruthValue.Undetermined)  // U: no collapse path — SC_PC violated
            .Aggregate(TruthValue.True, Operators.And),

        // [do τ]A: dynamic modality — parallel to σ over R_τ.
        // Semantics: min over R_τ-successors (parallel to □ over R_obs).
        // Vacuous case (no R_τ-successors): U rather than T, consistent with σ.
        // Rationale: an action with no effects is ill-formed, not vacuously true.
        DoOperator doOp => Frame.CollapseSuccessors(world)
            .Select(v => Evaluate(doOp.Sub, v))
            .DefaultIfEmpty(TruthValue.Undetermined)  // U: no action targets — ill-formed
            .Aggregate(TruthValue.True, Operators.And),

        _ => throw new NotSupportedException($"Unknown formula type: {formula.GetType().Name}")
    };

    /// <summary>
    /// Evaluates obs_w(A): observability of A at world w.
    ///
    /// Semantics: obs_w(A) = T iff A has the same value at all R_obs-accessible worlds.
    ///            obs_w(A) = U iff A is U at w (ontological constraint).
    ///            obs_w(A) = U iff A has different values at some accessible worlds.
    ///            obs_w(A) = F is not possible when A is F... (see constraints in ObsOperator).
    ///
    /// NOTE: This is one canonical implementation. The obs function is under-determined
    /// by the axioms (F4 open). Different models may use different obs functions.
    /// </summary>
    private TruthValue EvaluateObs(Formula sub, World world)
    {
        var baseValue = Evaluate(sub, world);

        // Ontological constraint: U is never directly observable
        if (baseValue == TruthValue.Undetermined)
            return TruthValue.Undetermined;

        var successors = Frame.ObsSuccessors(world).ToList();

        // No observation relation from w: treat as directly observable
        if (successors.Count == 0)
            return baseValue;

        // Check if all accessible worlds agree on the value.
        // If so, the formula's truth value is uniformly observed → observable.
        // obs_w(A) = baseValue  (not hardcoded T):
        //   - baseValue=T and all successors=T → obs=T  (truth is observable)
        //   - baseValue=F and all successors=F → obs=F  (falsity is observable)
        // Returning T for the F-case would violate: obs_w(F) ∈ {F, U} (Paper II §4).
        var values = successors.Select(v => Evaluate(sub, v)).ToHashSet();

        if (values.Count == 1 && values.First() == baseValue)
            return baseValue;  // uniformly observable — return the observed truth value

        if (values.Contains(TruthValue.Undetermined))
            return TruthValue.Undetermined;  // observation touches superposition

        // Mixed values across accessible worlds → not fully observable
        return TruthValue.Undetermined;
    }

    /// <summary>
    /// Checks if a formula is valid (true at all worlds).
    /// Used for checking if φ is a theorem of the model.
    /// </summary>
    public bool IsValid(Formula formula) =>
        Frame.Worlds.All(w => Evaluate(formula, w) == TruthValue.True);

    /// <summary>
    /// Checks if φ is satisfiable (true at some world).
    /// </summary>
    public bool IsSatisfiable(Formula formula) =>
        Frame.Worlds.Any(w => Evaluate(formula, w) == TruthValue.True);

    /// <summary>
    /// Returns the WorldType of w under Option B semantics.
    /// SC_PC as theorem: every W_super has a collapse path to W_eigen.
    /// </summary>
    public WorldType GetWorldType(World w) => Valuation.GetWorldType(w);

    /// <summary>
    /// Verifies SC_PC (Superposition Collapse Principle) as a theorem.
    /// Every W_super world must have at least one R_τ successor in W_eigen.
    /// Under Option B, this should hold by construction; violations indicate model error.
    /// </summary>
    public bool VerifySC_PC() =>
        Frame.Worlds
            .Where(w => GetWorldType(w) == WorldType.Super)
            .All(w => Frame.CollapseSuccessors(w)
                .Any(v => GetWorldType(v) == WorldType.Eigen));
}
