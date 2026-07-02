using ECL3Q.Agents;
using ECL3Q.Core.Semantics;

namespace ECL3Q.Taxonomy;

/// <summary>
/// The 8 categories of σ-conflict in ECL₃^Q.
///
/// Formal axiomatization: Ax1–Ax7 (SR1016).
/// Minimality proven: exactly 5 minimal 4-dimension classification systems.
/// Completeness proven for SR726-rev.
///
/// The taxonomy covers all ways in which the σ-collapse structure can be violated.
/// Categories are DISJOINT (Ax4) and COMPLETE (Ax5) for the domain of σ-conflicts.
///
/// See Paper XVII (target: Studia Logica / J. Applied Logic) for full treatment.
/// </summary>
public enum SigmaConflictCategory
{
    /// <summary>
    /// σ-Hierarchie-Konflikt: Competing, incompatible σ-collapse hierarchies.
    /// Multiple agents claim legitimate σ-carrier authority at the same level.
    /// Example: two courts both claim final jurisdiction over the same decision.
    /// Ax: ∃ agents i,j: both are σ-carriers AND their R_τ relations conflict.
    /// </summary>
    HierarchieKonflikt,

    /// <summary>
    /// σ-Vakuum: No legitimate σ-carrier present.
    /// The collapse should occur but no agent is authorized to perform it.
    /// Example: legal gap — situation requires a decision but no institution has authority.
    /// Ax: W_super world exists, but no agent has σ-carrier status for it.
    /// </summary>
    Vakuum,

    /// <summary>
    /// σ-Inversion: Collapse occurs in the wrong direction.
    /// A W_eigen world is "uncollapsed" back to W_super (forbidden by σ-irreversibility).
    /// Example: a final court ruling is re-opened as if undecided.
    /// Ax: ∃ w ∈ W_eigen, v ∈ W_super: w R_τ v (inverse collapse relation).
    /// </summary>
    Inversion,

    /// <summary>
    /// σ-Unterwanderung: Formal carrier, but no actual collapse occurs.
    /// An agent is designated as σ-carrier but does not exercise the authority.
    /// Example: a committee that is formally responsible but perpetually defers.
    /// Ax: agent i is σ-carrier for w, but w remains in W_super indefinitely.
    /// </summary>
    Unterwanderung,

    /// <summary>
    /// σ-Vorab-Kollaps: Collapse before legitimate observation.
    /// A σ-collapse occurs before the observational preconditions are met.
    /// Example: a verdict before the evidence is examined (premature conclusion).
    /// Ax: σ(A) occurs at w, but ¬Obs(A) at w (A not yet observable).
    /// </summary>
    VorabKollaps,

    /// <summary>
    /// σ-Delegation: Unauthorized transfer of σ-carrier status.
    /// A σ-carrier delegates collapse authority to a non-authorized agent.
    /// Example: a government delegates constitutional decisions to a private body.
    /// Ax: agent i (σ-carrier) transfers to agent j (non-carrier) without authorization.
    /// </summary>
    Delegation,

    /// <summary>
    /// σ-Kaskade: Propagation of collapse errors through the system.
    /// An initial σ-conflict generates secondary conflicts downstream.
    /// Example: an invalid collapse triggers further invalid collapses in dependent subsystems.
    /// Ax: σ-conflict at w₁ causes σ-conflict at w₂ where w₁ R_τ w₂.
    /// </summary>
    Kaskade,

    /// <summary>
    /// σ-Diskontinuität: Broken collapse chain.
    /// The collapse process is interrupted, leaving the system in W_super without resolution.
    /// Example: a decision process that is started but never completed (institutional paralysis).
    /// Ax: collapse chain w₀ R_τ w₁ R_τ ... is broken before reaching W_eigen.
    /// </summary>
    Diskontinuitaet
}

/// <summary>
/// Represents a detected σ-conflict instance.
/// </summary>
public record SigmaConflict(
    SigmaConflictCategory Category,
    string Description,
    World? PrimaryWorld = null,
    IReadOnlyList<Agent>? InvolvedAgents = null)
{
    public override string ToString() =>
        $"σ-Konflikt [{Category}]: {Description}" +
        (PrimaryWorld != null ? $" @ {PrimaryWorld}" : "") +
        (InvolvedAgents?.Any() == true
            ? $" (Agents: {string.Join(", ", InvolvedAgents.Select(a => a.Id))})"
            : "");
}

/// <summary>
/// σ-Conflict detector: analyzes a multi-agent model for σ-taxonomy violations.
/// Implements the formal axioms Ax1–Ax7.
///
/// Ax1: Well-formedness (implicit — frames must satisfy basic ECL₃^Q conditions)
/// Ax2: Each conflict belongs to exactly one category (Disjointness + Completeness)
/// Ax3: Categories are non-empty (each has at least one canonical instance)
/// Ax4: DISJOINTNESS — conflicts are classified into exactly one category
/// Ax5: COMPLETENESS — every σ-conflict falls into some category
/// Ax6: Minimality — no proper sub-taxonomy suffices (proven: 5 minimal 4-dim systems)
/// Ax7: Independence — removing any category leaves some conflicts unclassifiable
/// </summary>
public class SigmaConflictDetector
{
    private readonly MultiAgentFrame _frame;
    private readonly Model _model;

    public SigmaConflictDetector(MultiAgentFrame frame, Model model)
    {
        _frame = frame;
        _model = model;
    }

    /// <summary>
    /// Detects all σ-conflicts in the model. Returns list of detected conflicts.
    /// Order: Vakuum → Inversion → VorabKollaps → Delegation → Unterwanderung
    ///        → HierarchieKonflikt → Kaskade → Diskontinuität
    /// </summary>
    public IReadOnlyList<SigmaConflict> DetectAll()
    {
        var conflicts = new List<SigmaConflict>();

        conflicts.AddRange(DetectVakuum());
        conflicts.AddRange(DetectInversion());
        conflicts.AddRange(DetectVorabKollaps());
        conflicts.AddRange(DetectDelegation());
        conflicts.AddRange(DetectUnterwanderung());
        conflicts.AddRange(DetectHierarchieKonflikt());
        conflicts.AddRange(DetectKaskade());
        conflicts.AddRange(DetectDiskontinuitaet());

        return conflicts;
    }

    /// <summary>σ-Vakuum: W_super world with no σ-carrier agent.</summary>
    private IEnumerable<SigmaConflict> DetectVakuum()
    {
        // Compute once — O(n) not O(n×m)
        bool hasCarrier = _frame.SigmaCarriers().Any();
        if (hasCarrier) yield break;

        foreach (var w in _frame.BaseFrame.Worlds.Where(
                     w => _model.GetWorldType(w) == WorldType.Super))
            yield return new SigmaConflict(
                SigmaConflictCategory.Vakuum,
                $"No σ-carrier exists for superposition world {w}",
                w);
    }

    /// <summary>σ-Inversion: W_eigen world with R_τ successor in W_super.</summary>
    private IEnumerable<SigmaConflict> DetectInversion()
    {
        foreach (var w in _frame.BaseFrame.Worlds)
        {
            if (_model.GetWorldType(w) != WorldType.Eigen) continue;

            foreach (var v in _frame.BaseFrame.CollapseSuccessors(w))
            {
                if (_model.GetWorldType(v) == WorldType.Super)
                    yield return new SigmaConflict(
                        SigmaConflictCategory.Inversion,
                        $"Inverse collapse: W_eigen {w} → W_super {v}",
                        w);
            }
        }
    }

    /// <summary>σ-Vorab-Kollaps: collapse before observation is established.</summary>
    private IEnumerable<SigmaConflict> DetectVorabKollaps()
    {
        // Check: W_super worlds with collapse targets where the collapsing formula
        // is not yet observable (obs_w = U for relevant propositions)
        // TODO: Requires formula-level analysis; structural check only here
        foreach (var w in _frame.BaseFrame.Worlds)
        {
            if (_model.GetWorldType(w) != WorldType.Super) continue;

            // If world has no observation successors but has collapse successors:
            // collapse before observation structure established
            bool hasObsSuccessors = _frame.BaseFrame.ObsSuccessors(w).Any();
            bool hasCollapseSuccessors = _frame.BaseFrame.CollapseSuccessors(w).Any();

            if (hasCollapseSuccessors && !hasObsSuccessors)
                yield return new SigmaConflict(
                    SigmaConflictCategory.VorabKollaps,
                    $"Collapse at {w} without observation relation (premature collapse)",
                    w);
        }
    }

    /// <summary>
    /// σ-Delegation: unauthorized transfer of σ-carrier status.
    ///
    /// True delegation requires an authorization hierarchy: agent i (authorized)
    /// transfers to agent j (not independently authorized). Multiple carriers at the
    /// same level is σ-Hierarchie-Konflikt, not Delegation.
    ///
    /// Without an explicit authorization hierarchy in the model, Delegation cannot
    /// be structurally detected and is conservatively not reported.
    /// Callers may inject an authorization predicate via the overload below.
    /// </summary>
    private IEnumerable<SigmaConflict> DetectDelegation() =>
        DetectDelegation(authorizedCarriers: null);

    /// <summary>
    /// Detect σ-Delegation given an explicit set of independently authorized carrier IDs.
    /// Agents in <paramref name="authorizedCarriers"/> are legitimately authorized;
    /// all other σ-carriers are delegated-to (potentially unauthorized).
    /// </summary>
    public IEnumerable<SigmaConflict> DetectDelegation(IReadOnlySet<string>? authorizedCarriers)
    {
        if (authorizedCarriers == null) yield break; // Cannot detect without authorization model

        foreach (var carrier in _frame.SigmaCarriers())
        {
            if (!authorizedCarriers.Contains(carrier.Id))
                yield return new SigmaConflict(
                    SigmaConflictCategory.Delegation,
                    $"Agent '{carrier.Id}' holds σ-carrier status without independent authorization",
                    InvolvedAgents: [carrier]);
        }
    }

    /// <summary>σ-Unterwanderung: carrier exists but W_super remains unresolved.</summary>
    private IEnumerable<SigmaConflict> DetectUnterwanderung()
    {
        var carriers = _frame.SigmaCarriers().ToList();
        if (!carriers.Any()) yield break;

        foreach (var w in _frame.BaseFrame.Worlds)
        {
            if (_model.GetWorldType(w) != WorldType.Super) continue;

            if (!_frame.BaseFrame.CollapseSuccessors(w).Any())
                yield return new SigmaConflict(
                    SigmaConflictCategory.Unterwanderung,
                    $"σ-carrier(s) exist but {w} remains in W_super with no collapse path",
                    w, carriers);
        }
    }

    /// <summary>σ-Hierarchie-Konflikt: competing carrier hierarchies.</summary>
    private IEnumerable<SigmaConflict> DetectHierarchieKonflikt()
    {
        var carriers = _frame.SigmaCarriers().ToList();
        if (carriers.Count < 2) yield break;

        // Only check W_super worlds — conflicts at W_eigen worlds are irrelevant
        // (the collapse has already happened there).
        foreach (var w in _frame.BaseFrame.Worlds
                     .Where(w => _model.GetWorldType(w) == WorldType.Super))
        {
            var successorSets = carriers
                .Select(a => a.ObsSuccessors(w).ToHashSet())
                .ToList();

            // Skip agents with no individual R_obs from w (empty sets are equal —
            // they simply have no opinion, which is not a conflict).
            var nonEmpty = successorSets.Where(s => s.Count > 0).ToList();
            if (nonEmpty.Count < 2) continue;

            var first = nonEmpty[0];
            if (nonEmpty.Skip(1).Any(s => !s.SetEquals(first)))
                yield return new SigmaConflict(
                    SigmaConflictCategory.HierarchieKonflikt,
                    $"Carriers have conflicting observation relations at {w}",
                    w, carriers);
        }
    }

    /// <summary>σ-Kaskade: downstream propagation of collapse errors.</summary>
    private IEnumerable<SigmaConflict> DetectKaskade()
    {
        // Detect W_super worlds reachable from other W_super worlds via R_τ
        // (cascade: invalid collapse propagates)
        var superWorlds = _frame.BaseFrame.Worlds
            .Where(w => _model.GetWorldType(w) == WorldType.Super)
            .ToHashSet();

        foreach (var w in superWorlds)
        {
            foreach (var v in _frame.BaseFrame.CollapseSuccessors(w))
            {
                if (superWorlds.Contains(v))
                    yield return new SigmaConflict(
                        SigmaConflictCategory.Kaskade,
                        $"Cascade: W_super {w} collapses to W_super {v} (no resolution)",
                        w);
            }
        }
    }

    /// <summary>
    /// σ-Diskontinuität: broken collapse chain — W_super has successors but no path to W_eigen.
    ///
    /// Distinction from σ-Unterwanderung:
    ///   Unterwanderung: W_super has NO collapse successors at all (carrier does nothing).
    ///   Diskontinuität: W_super has successors, but the chain never reaches W_eigen
    ///                   (e.g. all successors are also W_super — typically also a Kaskade).
    /// These are disjoint by definition (Ax4).
    /// </summary>
    private IEnumerable<SigmaConflict> DetectDiskontinuitaet()
    {
        var superWorlds = _frame.BaseFrame.Worlds
            .Where(w => _model.GetWorldType(w) == WorldType.Super)
            .ToList();

        foreach (var w in superWorlds)
        {
            // Only report Diskontinuität when there ARE successors but no W_eigen path.
            // W_super with no successors is Unterwanderung or Vakuum — not Diskontinuität.
            if (!_frame.BaseFrame.CollapseSuccessors(w).Any()) continue;

            if (!HasPathToEigen(w, new HashSet<World>()))
                yield return new SigmaConflict(
                    SigmaConflictCategory.Diskontinuitaet,
                    $"Broken collapse chain: {w} has successors but no path to any W_eigen world",
                    w);
        }
    }

    private bool HasPathToEigen(World w, HashSet<World> visited)
    {
        if (visited.Contains(w)) return false;
        visited.Add(w);

        if (_model.GetWorldType(w) == WorldType.Eigen) return true;

        return _frame.BaseFrame.CollapseSuccessors(w)
            .Any(v => HasPathToEigen(v, visited));
    }
}
