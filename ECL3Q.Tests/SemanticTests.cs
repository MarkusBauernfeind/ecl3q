using ECL3Q.Core.Algebra;
using ECL3Q.Core.Inference;
using ECL3Q.Core.Semantics;
using ECL3Q.Core.Syntax;
using Xunit;
using static ECL3Q.Core.Syntax.F;

namespace ECL3Q.Tests;

/// <summary>
/// Tests verifying semantic properties of ECL₃^Q models.
/// Corresponds to Handover §9 test requirements.
/// </summary>
public class SemanticTests
{
    // ── Classical Logic Containment ───────────────────────────────────────────

    [Fact]
    public void ClassicalTautologies_AreValidWhenUExcluded()
    {
        // If U is excluded, ECL₃^Q collapses to classical logic
        var excluded = new[] { TruthValue.False, TruthValue.True };

        // Law of excluded middle: p ∨ ¬p
        var lem = Or(P, Not(P));
        var atoms = lem.Atoms().ToList();

        bool valid = ProofSearch.EnumerateClassicalAssignments(atoms)
            .All(a => ProofSearch.EvaluatePropositional(lem, a) == TruthValue.True);

        Assert.True(valid, "LEM should be classically valid");
    }

    [Fact]
    public void LEM_IsNotK3Tautology()
    {
        // p ∨ ¬p is NOT a K₃ tautology (U ∨ ¬U = U ∨ U = U ≠ T)
        var lem = Or(P, Not(P));
        Assert.False(ProofSearch.IsK3Tautology(lem),
            "LEM should NOT be a K₃ tautology (U∨¬U = U)");
    }

    [Fact]
    public void ClassicalTautologies_AreClassicallyValid()
    {
        // These are classically valid but NOT K₃-tautologies (U-assignments give U, not T).
        // K₃ has very few tautologies — formulas with → or ↔ almost never qualify
        // because U→U=U and U↔U=U (both ≠ T).
        var formula = Implies(And(P, Q), P);
        Assert.True(ProofSearch.IsClassicalTautology(formula));
        Assert.False(ProofSearch.IsK3Tautology(formula));  // p=U,q=U → (U∧U)→U = U→U = U
    }

    [Fact]
    public void DoubleNegation_IsClassicallyValid_NotK3()
    {
        // ¬¬p ↔ p is classically valid (involution holds classically).
        // NOT a K₃-tautology: p=U → ¬¬U=U, U↔U = min(U→U, U→U) = min(U,U) = U ≠ T
        var dn = Iff(Not(Not(P)), P);
        Assert.True(ProofSearch.IsClassicalTautology(dn));
        Assert.False(ProofSearch.IsK3Tautology(dn));
    }

    [Fact]
    public void NonContradiction_IsNotK3Tautology()
    {
        // ¬(p ∧ ¬p) is NOT a K₃ tautology: p=U → U∧U=U → ¬U=U ≠ T
        var nc = Not(And(P, Not(P)));
        Assert.False(ProofSearch.IsK3Tautology(nc));
    }

    // ── Modal Frame Properties ────────────────────────────────────────────────

    [Fact]
    public void RF1_ReflexiveFrame_SatisfiesRF1()
    {
        var w = new World("w");
        var frame = new Frame(
            worlds: [w],
            rObs: [(w, w)]);  // reflexive

        Assert.True(frame.SatisfiesRF1(), "Reflexive frame should satisfy RF1");
    }

    [Fact]
    public void RF1_NonReflexiveFrame_FailsRF1()
    {
        var w = new World("w");
        var v = new World("v");
        var frame = new Frame(
            worlds: [w, v],
            rObs: [(w, v)]);  // not reflexive (w not related to w)

        Assert.False(frame.SatisfiesRF1(), "Non-reflexive frame should fail RF1");
    }

    [Fact]
    public void RF3_TransitiveFrame_SatisfiesRF3()
    {
        var w = new World("w");
        var v = new World("v");
        var u = new World("u");
        var frame = new Frame(
            worlds: [w, v, u],
            rObs: [(w, v), (v, u), (w, u)]);  // transitive closure included

        Assert.True(frame.SatisfiesRF3(), "Transitive frame should satisfy RF3");
    }

    [Fact]
    public void RF3_NonTransitiveFrame_FailsRF3()
    {
        var w = new World("w");
        var v = new World("v");
        var u = new World("u");
        var frame = new Frame(
            worlds: [w, v, u],
            rObs: [(w, v), (v, u)]);  // missing (w,u) — not transitive

        Assert.False(frame.SatisfiesRF3(), "Non-transitive frame should fail RF3");
    }

    // ── Option B: World Type Determination ────────────────────────────────────

    [Fact]
    public void WorldType_Super_WhenActionVarIsU()
    {
        var w = new World("w");
        var frame = new Frame([w]);

        var val = new Valuation(
            assignments: [
                (("τ", w), TruthValue.Undetermined)
            ],
            actionVariables: ["τ"]);

        var model = new Model(frame, val);
        Assert.Equal(WorldType.Super, model.GetWorldType(w));
    }

    [Fact]
    public void WorldType_Eigen_WhenAllActionVarsClassical()
    {
        var w = new World("w");
        var frame = new Frame([w]);

        var val = new Valuation(
            assignments: [
                (("τ", w), TruthValue.True)
            ],
            actionVariables: ["τ"]);

        var model = new Model(frame, val);
        Assert.Equal(WorldType.Eigen, model.GetWorldType(w));
    }

    // ── SC_PC Verification ────────────────────────────────────────────────────

    [Fact]
    public void SCPC_Holds_WhenSuperWorldHasEigenSuccessor()
    {
        var wSuper = new World("w_super");
        var wEigen = new World("w_eigen");
        var frame = new Frame(
            worlds: [wSuper, wEigen],
            rTau: [(wSuper, wEigen)]);

        var val = new Valuation(
            assignments: [
                (("τ", wSuper), TruthValue.Undetermined),
                (("τ", wEigen), TruthValue.True)
            ],
            actionVariables: ["τ"]);

        var model = new Model(frame, val);
        Assert.True(model.VerifySC_PC());
    }

    [Fact]
    public void SCPC_Fails_WhenSuperWorldHasNoCollapseTarget()
    {
        var wSuper = new World("w_super");
        var frame = new Frame([wSuper]);  // no R_τ successors

        var val = new Valuation(
            assignments: [
                (("τ", wSuper), TruthValue.Undetermined)
            ],
            actionVariables: ["τ"]);

        var model = new Model(frame, val);
        Assert.False(model.VerifySC_PC());
    }

    // ── Formula Evaluation in Models ──────────────────────────────────────────

    [Fact]
    public void EvaluateObs_ObsOfFalse_ReturnsF_WhenUniformlyFalse()
    {
        // Constraint: obs_w(F) ∈ {F, U} — never T.
        // When A=F locally and all successors also have A=F → obs = F (not T).
        var w  = new World("w");
        var v1 = new World("v1");
        var v2 = new World("v2");
        var frame = new Frame(
            worlds: [w, v1, v2],
            rObs: [(w, v1), (w, v2)]);
        var val = new Valuation([
            (("p", w),  TruthValue.False),
            (("p", v1), TruthValue.False),
            (("p", v2), TruthValue.False),
        ]);
        var model = new Model(frame, val);
        var obsP = new ECL3Q.Core.Syntax.ObsFormula(new ECL3Q.Core.Syntax.Atom("p"));

        var result = model.Evaluate(obsP, w);
        // obs_w(F) must be F (uniform falsity is observable as false), not T.
        Assert.Equal(TruthValue.False, result);
        // Constraint check: obs_w(F) ∈ {F, U}
        Assert.NotEqual(TruthValue.True, result);
    }

    [Fact]
    public void EvaluateObs_ObsOfTrue_ReturnsT_WhenUniformlyTrue()
    {
        // obs_w(T) = T when all successors agree.
        var w  = new World("w");
        var v  = new World("v");
        var frame = new Frame(worlds: [w, v], rObs: [(w, v)]);
        var val = new Valuation([
            (("p", w), TruthValue.True),
            (("p", v), TruthValue.True),
        ]);
        var model = new Model(frame, val);
        var obsP = new ECL3Q.Core.Syntax.ObsFormula(new ECL3Q.Core.Syntax.Atom("p"));
        Assert.Equal(TruthValue.True, model.Evaluate(obsP, w));
    }

    [Fact]
    public void EvaluateObs_ObsOfTrue_ReturnsU_WhenSuccessorsDisagree()
    {
        // obs_w(T) = U when successors disagree (Source 2 of U:Obs).
        var w  = new World("w");
        var v1 = new World("v1");
        var v2 = new World("v2");
        var frame = new Frame(
            worlds: [w, v1, v2],
            rObs: [(w, v1), (w, v2)]);
        var val = new Valuation([
            (("p", w),  TruthValue.True),
            (("p", v1), TruthValue.True),
            (("p", v2), TruthValue.False),  // disagreement
        ]);
        var model = new Model(frame, val);
        var obsP = new ECL3Q.Core.Syntax.ObsFormula(new ECL3Q.Core.Syntax.Atom("p"));
        // Mixed successors → not uniformly observable → U
        Assert.Equal(TruthValue.Undetermined, model.Evaluate(obsP, w));
    }

    [Fact]
    public void Conjunction_EvaluatesCorrectly_InModel()
    {
        var w = new World("w");
        var frame = new Frame([w]);
        var val = new Valuation([
            (("p", w), TruthValue.True),
            (("q", w), TruthValue.Undetermined)
        ]);
        var model = new Model(frame, val);

        var pAndQ = And(P, Q);
        // T ∧ U = U
        Assert.Equal(TruthValue.Undetermined, model.Evaluate(pAndQ, w));
    }

    [Fact]
    public void DoOperator_WithNoSuccessors_ReturnsU()
    {
        // [do τ]φ with no R_τ successors = U (ill-formed action, not vacuously true).
        // Consistent with σ(φ) semantics (SC_PC violation → U).
        var w = new World("w");
        var frame = new Frame([w]); // no R_τ edges
        var val = new Valuation([(("p", w), TruthValue.True)]);
        var model = new Model(frame, val);
        var doP = new ECL3Q.Core.Syntax.DoOperator("τ", new ECL3Q.Core.Syntax.Atom("p"));
        Assert.Equal(TruthValue.Undetermined, model.Evaluate(doP, w));
    }

    [Fact]
    public void Collapse_WithNoSuccessors_ReturnsU()
    {
        // σ(φ) with no R_τ successors = U (SC_PC violated).
        var w = new World("w");
        var frame = new Frame([w]);
        var val = new Valuation([(("p", w), TruthValue.True)]);
        var model = new Model(frame, val);
        var colP = new ECL3Q.Core.Syntax.CollapseFormula(new ECL3Q.Core.Syntax.Atom("p"));
        Assert.Equal(TruthValue.Undetermined, model.Evaluate(colP, w));
    }

    [Fact]
    public void DoOperator_And_Collapse_AreSemanticallySameOverRTau()
    {
        // [do τ]φ and σ(φ) use the same R_τ relation and same min-semantics.
        // They should produce identical results for the same formula and world.
        var w = new World("w");
        var v = new World("v");
        var frame = new Frame(worlds: [w, v], rTau: [(w, v)]);
        var val = new Valuation([(("p", v), TruthValue.True)]);
        var model = new Model(frame, val);
        var p = new ECL3Q.Core.Syntax.Atom("p");
        var doP  = new ECL3Q.Core.Syntax.DoOperator("τ", p);
        var colP = new ECL3Q.Core.Syntax.CollapseFormula(p);
        Assert.Equal(model.Evaluate(doP, w), model.Evaluate(colP, w));
    }

    [Fact]
    public void ModalBox_EvaluatesAsMin_OverSuccessors()
    {
        var w  = new World("w");
        var v1 = new World("v1");
        var v2 = new World("v2");
        var frame = new Frame(
            worlds: [w, v1, v2],
            rObs: [(w, v1), (w, v2)]);

        var val = new Valuation([
            (("p", v1), TruthValue.True),
            (("p", v2), TruthValue.Undetermined)
        ]);
        var model = new Model(frame, val);

        // □p at w: min(T, U) = U
        Assert.Equal(TruthValue.Undetermined, model.Evaluate(Box(P), w));
    }
}
