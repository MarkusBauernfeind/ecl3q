using ECL3Q.Core.Algebra;
using ECL3Q.Core.Semantics;
using Xunit;

namespace ECL3Q.Tests;

public class EntanglementTests
{
    [Fact]
    public void EntangledCollapse_AntiCorrelated_ProducesOppositeValues()
    {
        // EPR-style: two entangled particles, anti-correlated
        var w1 = new World("particle_1");
        var w2 = new World("particle_2");
        var frame = new Frame([w1, w2]);

        var val = new Valuation(
            assignments: [
                (("spin", w1), TruthValue.Undetermined),
                (("spin", w2), TruthValue.Undetermined)
            ],
            actionVariables: ["spin"]);

        var model = new Model(frame, val);
        var entangled = new EntangledModel(model);

        // Anti-correlation: if w1 collapses to T, w2 must be F
        entangled.AddEntanglement(w1, w2, "spin",
            v => v == TruthValue.True ? TruthValue.False : TruthValue.True);

        var results = entangled.CollapseAndPropagate(w1, "spin", TruthValue.True);

        Assert.Equal(2, results.Count);
        var w1Result = results.First(r => r.World == w1);
        var w2Result = results.First(r => r.World == w2);

        Assert.Equal(TruthValue.True, w1Result.Value);
        Assert.Equal(TruthValue.False, w2Result.Value);  // anti-correlated
    }

    [Fact]
    public void EntangledCollapse_Correlated_ProducesSameValue()
    {
        var w1 = new World("particle_1");
        var w2 = new World("particle_2");
        var frame = new Frame([w1, w2]);

        var val = new Valuation(
            assignments: [
                (("state", w1), TruthValue.Undetermined),
                (("state", w2), TruthValue.Undetermined)
            ],
            actionVariables: ["state"]);

        var model = new Model(frame, val);
        var entangled = new EntangledModel(model);

        // Perfect correlation: same value
        entangled.AddEntanglement(w1, w2, "state", v => v);

        var results = entangled.CollapseAndPropagate(w1, "state", TruthValue.False);

        var w2Result = results.First(r => r.World == w2);
        Assert.Equal(TruthValue.False, w2Result.Value);
    }

    [Fact]
    public void Collapse_WithUResult_Throws()
    {
        // σ-collapse result cannot be U — collapse resolves ontological indeterminacy
        var w = new World("w");
        var frame = new Frame([w]);
        var val = new Valuation([(("p", w), TruthValue.Undetermined)]);
        var model = new Model(frame, val);
        var entangled = new EntangledModel(model);

        Assert.Throws<ArgumentException>(() =>
            entangled.CollapseAndPropagate(w, "p", TruthValue.Undetermined));
    }

    [Fact]
    public void NoSignaling_PreCollapse_IsAlwaysTrue()
    {
        // SR74: No-signaling — U propositions are not observable
        // regardless of entanglement
        var w1 = new World("w1");
        var w2 = new World("w2");
        var frame = new Frame([w1, w2]);

        var val = new Valuation(
            assignments: [
                (("p", w1), TruthValue.Undetermined),
                (("p", w2), TruthValue.Undetermined)
            ],
            actionVariables: ["p"]);

        var model = new Model(frame, val);
        var entangled = new EntangledModel(model);
        entangled.AddEntanglement(w1, w2, "p", v => v);

        // No signaling must hold pre-collapse
        Assert.True(entangled.VerifyNoSignaling(w1, "p"));
        Assert.True(entangled.VerifyNoSignaling(w2, "p"));
    }

    [Fact]
    public void Decoherence_ProducesSingleResult_NoPropagation()
    {
        // SR82: Decoherence dissolves entanglement — no partner correlation
        var w = new World("w");
        var frame = new Frame([w]);
        var val = new Valuation(
            assignments: [(("p", w), TruthValue.Undetermined)],
            actionVariables: ["p"]);

        var model = new Model(frame, val);
        var entangled = new EntangledModel(model);

        var results = entangled.Decohere(w, "p", TruthValue.True);

        // Only the decohered world — no partners
        Assert.Single(results);
        Assert.Equal(TruthValue.True, results[0].Value);
    }
}
