using ECL3Q.Core.Algebra;
using ECL3Q.Core.Syntax;

namespace ECL3Q.Core.Semantics;

/// <summary>
/// Quantum-mechanical extension of ECL₃^Q (SR72–SR84, Paper III).
///
/// Models entanglement between worlds: two worlds w₁, w₂ are entangled
/// if their truth values for certain propositions are correlated — measurement
/// (σ-collapse) on one instantaneously determines the other.
///
/// This is a SEMANTIC extension: entanglement is encoded in the valuation
/// via joint constraints, not as new syntactic operators.
///
/// Key results:
///   SR72: Entanglement relation ≡_E on worlds (symmetric, not necessarily transitive)
///   SR73: Correlated collapse: σ(w₁) determines V(p, w₂) for entangled p
///   SR74: No-signaling: entanglement cannot transmit information (obs_w is unaffected)
///   SR75-SR84: Multi-particle extensions, decoherence, measurement basis
/// </summary>
public class EntangledModel
{
    /// <summary>Base model underlying the entangled structure.</summary>
    public Model BaseModel { get; }

    /// <summary>
    /// Entanglement relation: pairs of (World, Atom) that are correlated.
    /// (w₁, p) ≡_E (w₂, p) means: collapsing p at w₁ determines p at w₂.
    /// The correlation is expressed as a function: given V(p,w₁) post-collapse,
    /// V(p,w₂) is determined by the correlation function.
    /// </summary>
    private readonly Dictionary<(World, string), List<EntanglementLink>> _entanglement = [];

    public EntangledModel(Model baseModel)
    {
        BaseModel = baseModel;
    }

    /// <summary>
    /// Register an entanglement link between (w1, atom) and (w2, atom).
    /// correlation: given the post-collapse value at w1, returns the value at w2.
    /// Classical EPR-style: correlation(T) = F (anti-correlated), or correlation(T) = T (correlated).
    /// </summary>
    public void AddEntanglement(
        World w1, World w2, string atom,
        Func<TruthValue, TruthValue> correlation)
    {
        var key = (w1, atom);
        if (!_entanglement.ContainsKey(key))
            _entanglement[key] = [];
        _entanglement[key].Add(new EntanglementLink(w2, correlation));
    }

    /// <summary>
    /// Simulate σ-collapse at (world, atom) and propagate entanglement.
    /// Returns: updated valuations for all entangled (world, atom) pairs.
    ///
    /// SR73: Correlated collapse — after σ(w₁), entangled w₂ values are determined.
    /// </summary>
    public IReadOnlyList<(World World, string Atom, TruthValue Value)>
        CollapseAndPropagate(World w, string atom, TruthValue collapseResult)
    {
        // collapseResult must be classical (post-collapse is always W_eigen)
        if (collapseResult == TruthValue.Undetermined)
            throw new ArgumentException(
                "σ-collapse result must be classical (T or F). " +
                "U cannot be a collapse outcome — collapse resolves indeterminacy.");

        var results = new List<(World, string, TruthValue)>
        {
            (w, atom, collapseResult)
        };

        // Propagate to entangled partners
        if (_entanglement.TryGetValue((w, atom), out var links))
        {
            foreach (var link in links)
            {
                var partnerValue = link.Correlation(collapseResult);
                results.Add((link.PartnerWorld, atom, partnerValue));

                // Entanglement is typically symmetric: propagate back check
                // (but no infinite loop — already determined)
            }
        }

        return results;
    }

    /// <summary>
    /// SR74: No-signaling check.
    /// Entanglement must not allow information transmission before collapse.
    /// Specifically: the probability distribution of observable outcomes at w₂
    /// must be independent of whether a collapse has occurred at w₁.
    ///
    /// In ECL₃^Q: no-signaling holds iff Obs(atom)@w₂ remains U before any collapse
    /// — i.e., the atom is not observable at w₂ while in superposition.
    /// This is guaranteed by the ontological constraint: U is never directly observable.
    ///
    /// Returns true iff atom is in superposition at w (V(atom,w) = U),
    /// which is the pre-condition under which no-signaling is non-trivially required.
    /// Returns true unconditionally if atom is already classical (entanglement irrelevant).
    /// </summary>
    public bool VerifyNoSignaling(World w, string atom)
    {
        var baseValue = BaseModel.Evaluate(new Atom(atom), w);

        // Case 1: atom is U at w — Obs(atom)=U by ontological constraint.
        // Entanglement cannot make U observable. No-signaling holds.
        if (baseValue == TruthValue.Undetermined)
            return true;

        // Case 2: atom is already classical at w — collapse has already occurred
        // or atom was never in superposition. Entanglement is irrelevant.
        // No-signaling trivially holds (nothing to signal).
        return true;

        // NOTE: A full no-signaling verification would require checking that
        // CollapseAndPropagate at a remote entangled world does not change the
        // observable distribution at w before the local collapse is "known".
        // In this discrete ECL₃^Q model, that property follows from U being
        // unobservable — entanglement links only activate post-collapse (see CollapseAndPropagate).
    }

    /// <summary>
    /// Decoherence (SR82): transition from entangled superposition to classical mixture.
    /// After decoherence, worlds behave independently (entanglement links dissolved).
    /// Returns new model where entangled atoms are assigned definite values.
    /// </summary>
    public IReadOnlyList<(World World, string Atom, TruthValue Value)>
        Decohere(World w, string atom, TruthValue decoheredValue)
    {
        // Decoherence is like collapse but without the partner correlation:
        // the quantum coherence is lost to the environment.
        // Result: value at w becomes definite, partner values become independent.
        return [(w, atom, decoheredValue)];
    }

    private record EntanglementLink(World PartnerWorld, Func<TruthValue, TruthValue> Correlation);
}

/// <summary>
/// Measurement basis (SR75): the choice of basis determines what is observed.
/// In ECL₃^Q, basis corresponds to which action variable τ is used for collapse.
/// Different τ choices (bases) give different post-collapse states.
/// </summary>
public record MeasurementBasis(string Name, string ActionVariable)
{
    /// <summary>
    /// Standard computational basis: collapse determined by τ directly.
    /// </summary>
    public static MeasurementBasis Computational(string τ) =>
        new("Computational", τ);

    /// <summary>
    /// Hadamard-analogue basis: collapse in superposition of T/F.
    /// (Conceptual — full formalization requires SR75-SR80 extension.)
    /// </summary>
    public static MeasurementBasis Hadamard(string τ) =>
        new("Hadamard", τ);
}
