using ECL3Q.Core.Algebra;
using ECL3Q.Core.Semantics;
using ECL3Q.Core.Syntax;

namespace ECL3Q.Agents;

/// <summary>
/// An epistemic agent in ECL₃^Q multi-agent framework.
///
/// Each agent has:
/// - An individual observation relation R_obs^i (possibly distinct from global R_obs)
/// - σ-carrier status: designated authority to perform collapse
/// - Individual observability function obs^i_w
///
/// See SR85–SR116 for multi-agent theory.
/// </summary>
public class Agent
{
    /// <summary>Unique identifier for this agent (e.g. "SupremeCourt", "Agent_A").</summary>
    public string Id { get; }

    /// <summary>
    /// Whether this agent is a σ-carrier (has authority to perform collapse).
    /// σ-carrier is the designated agent with legitimate collapse authority.
    /// Relates to σ-Taxonomie: violation of carrier structure → σ-Konflikt.
    /// </summary>
    public bool IsSigmaCarrier { get; set; }

    /// <summary>
    /// Agent-specific observation relation (subset of global R_obs).
    /// Agent i can only observe via its own R_obs^i.
    /// </summary>
    private readonly HashSet<(World From, World To)> _individualRObs;

    /// <summary>
    /// Creates an agent with the given identity, σ-carrier status, and
    /// individual observation relation.
    /// </summary>
    /// <param name="id">Unique agent identifier.</param>
    /// <param name="isSigmaCarrier">Whether this agent holds σ-carrier authority.</param>
    /// <param name="individualRObs">
    /// Agent-specific R_obs pairs. If null, the agent has no individual observation relation
    /// (uses only the global frame relation when accessed via <see cref="MultiAgentFrame"/>).
    /// </param>
    public Agent(
        string id,
        bool isSigmaCarrier = false,
        IEnumerable<(World, World)>? individualRObs = null)
    {
        Id = id;
        IsSigmaCarrier = isSigmaCarrier;
        _individualRObs = individualRObs?.ToHashSet() ?? [];
    }

    /// <summary>The agent's individual observation relation as a read-only set of world pairs.</summary>
    public IReadOnlySet<(World From, World To)> IndividualRObs => _individualRObs;

    /// <summary>Returns all worlds observable by this agent from world <paramref name="w"/>.</summary>
    public IEnumerable<World> ObsSuccessors(World w) =>
        _individualRObs.Where(r => r.From == w).Select(r => r.To);

    public override string ToString() => $"Agent({Id}, σ-carrier={IsSigmaCarrier})";
}

/// <summary>
/// Multi-agent Kripke frame: extends <see cref="Frame"/> with per-agent observation relations.
/// Used for common knowledge computation and σ-conflict detection.
/// </summary>
public class MultiAgentFrame
{
    /// <summary>The underlying global Kripke frame (worlds, global R_obs, R_τ).</summary>
    public Frame BaseFrame { get; }

    /// <summary>All agents in this frame.</summary>
    public IReadOnlyList<Agent> Agents { get; }

    /// <summary>
    /// Creates a multi-agent frame from a global frame and a set of agents.
    /// Each agent may have its own individual R_obs^i distinct from the global R_obs.
    /// </summary>
    public MultiAgentFrame(Frame baseFrame, IEnumerable<Agent> agents)
    {
        BaseFrame = baseFrame;
        Agents = agents.ToList();
    }

    /// <summary>
    /// Common knowledge accessibility: w CK v iff for all agents i, w R_obs^i ... v.
    /// Common knowledge is the intersection of individual observation relations
    /// (transitive closure for full CK — here: direct intersection).
    /// </summary>
    public IEnumerable<World> CommonKnowledgeSuccessors(World w) =>
        Agents
            .Select(a => a.ObsSuccessors(w).ToHashSet())
            .Aggregate((a, b) => a.Intersect(b).ToHashSet());

    /// <summary>
    /// Returns all agents with σ-carrier status.
    /// In well-formed models: exactly one agent should be σ-carrier per collapse context.
    /// Multiple σ-carriers → σ-Hierarchie-Konflikt risk.
    /// </summary>
    public IEnumerable<Agent> SigmaCarriers() =>
        Agents.Where(a => a.IsSigmaCarrier);

    /// <summary>
    /// Evaluates formula φ at world w from agent i's perspective.
    /// Uses agent i's individual R_obs^i for modal operators.
    /// </summary>
    public TruthValue EvaluateForAgent(Model model, Formula formula, World w, Agent agent)
    {
        // For modal formulas, use agent-specific relation
        if (formula is ModalBox box)
            return agent.ObsSuccessors(w)
                .Select(v => model.Evaluate(box.Sub, v))
                .DefaultIfEmpty(TruthValue.True)
                .Aggregate(TruthValue.True, Operators.And);

        if (formula is ModalDiamond diamond)
            return agent.ObsSuccessors(w)
                .Select(v => model.Evaluate(diamond.Sub, v))
                .DefaultIfEmpty(TruthValue.False)
                .Aggregate(TruthValue.False, Operators.Or);

        // Non-modal formulas: agent-independent evaluation
        return model.Evaluate(formula, w);
    }
}
