namespace ECL3Q.Core.Semantics;

/// <summary>
/// World type distinguishing superposition worlds from eigenstates.
///
/// OPTION B ARCHITECTURE (current canonical architecture):
/// W_super and W_eigen are NOT primitive frame properties.
/// They are SEMANTICALLY DEFINED via the valuation V(τ, ·):
///   - w ∈ W_super iff there exists an action τ such that V(τ, w) = U
///   - w ∈ W_eigen iff for all actions τ: V(τ, w) ∈ {T, F}
///
/// Consequence: SC_PC and TQ1_RF are THEOREMS (not independent frame conditions).
/// Do not hardcode world type as a primitive — it must be computable from V.
/// </summary>
public enum WorldType
{
    /// <summary>
    /// Superposition world (W_super): at least one action variable has value U.
    /// Corresponds to quantum state before measurement/σ-collapse.
    /// </summary>
    Super,

    /// <summary>
    /// Eigenstate world (W_eigen): all action variables have classical values {T, F}.
    /// Corresponds to post-collapse state.
    /// </summary>
    Eigen
}

/// <summary>
/// A possible world in ECL₃^Q.
/// Worlds are identified by string Id; type is derived, not primitive (Option B).
/// </summary>
public record World(string Id)
{
    public override string ToString() => Id;
}

/// <summary>
/// A Kripke frame for ECL₃^Q.
/// Contains: a set of worlds, observation relation R_obs, collapse relation R_τ.
///
/// FRAME-DEFINABILITY (from Paper I §5, corrected via two-model method):
///   RF1: frame-definable in ECL₃^Q (reflexivity of R_obs)
///   RF3: frame-definable in ECL₃^Q
///   RF2: NOT frame-definable — no single axiom schema defines its frame class.
///        (van Benthem p-morphism argument was found incorrect and corrected.)
///
/// Completeness requires SC_PC as explicit frame condition PLUS AO1_eigen as axiom schema.
/// </summary>
public class Frame
{
    private readonly HashSet<World> _worlds;
    private readonly HashSet<(World From, World To)> _rObs;
    private readonly HashSet<(World From, World To)> _rTau;

    public Frame(
        IEnumerable<World> worlds,
        IEnumerable<(World, World)>? rObs = null,
        IEnumerable<(World, World)>? rTau = null)
    {
        _rObs  = rObs?.ToHashSet()  ?? [];
        _rTau  = rTau?.ToHashSet()  ?? [];

        // Include all worlds explicitly listed, plus any worlds appearing in relations
        // but not in the worlds list. This prevents silent closed-world false values
        // for worlds that are implicitly part of the frame structure.
        _worlds = worlds.ToHashSet();
        foreach (var (f, t) in _rObs)  { _worlds.Add(f); _worlds.Add(t); }
        foreach (var (f, t) in _rTau)  { _worlds.Add(f); _worlds.Add(t); }
    }

    public IReadOnlySet<World> Worlds => _worlds;

    /// <summary>Observation relation: w R_obs v means "from w, world v is accessible for observation".</summary>
    public IReadOnlySet<(World From, World To)> RObs => _rObs;

    /// <summary>Collapse relation: w R_τ v means "action τ can collapse w to v".</summary>
    public IReadOnlySet<(World From, World To)> RTau => _rTau;

    /// <summary>Returns all worlds observable from w (successors under R_obs).</summary>
    public IEnumerable<World> ObsSuccessors(World w) =>
        _rObs.Where(r => r.From == w).Select(r => r.To);

    /// <summary>Returns all collapse targets from w (successors under R_τ).</summary>
    public IEnumerable<World> CollapseSuccessors(World w) =>
        _rTau.Where(r => r.From == w).Select(r => r.To);

    /// <summary>
    /// Checks RF1: R_obs is reflexive (w R_obs w for all w).
    /// RF1 is frame-definable — corresponds to axiom □A → A (T-axiom).
    /// </summary>
    public bool SatisfiesRF1() =>
        _worlds.All(w => _rObs.Contains((w, w)));

    /// <summary>
    /// Checks RF3: R_obs is transitive (if w R_obs v and v R_obs u then w R_obs u).
    /// RF3 is frame-definable — corresponds to axiom □A → □□A (4-axiom).
    /// </summary>
    public bool SatisfiesRF3() =>
        _worlds.All(w =>
            ObsSuccessors(w).All(v =>
                ObsSuccessors(v).All(u =>
                    _rObs.Contains((w, u)))));

    /// <summary>
    /// Checks SC_PC (Superposition Collapse Principle):
    /// Every W_super world has at least one R_τ successor in W_eigen.
    ///
    /// Under Option B, W_super is determined by valuation (not here directly).
    /// This method checks the structural precondition: every world has a collapse target.
    /// Full SC_PC verification requires a Model (with Valuation).
    /// </summary>
    public bool HasCollapseTargetsForAllWorlds() =>
        _worlds.All(w => CollapseSuccessors(w).Any());
}
