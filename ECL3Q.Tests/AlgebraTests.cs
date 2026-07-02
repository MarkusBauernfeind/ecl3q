using ECL3Q.Core.Algebra;
using ECL3Q.Core.Semantics;
using ECL3Q.Core.Syntax;
using Xunit;
using static ECL3Q.Core.Algebra.TruthValue;

namespace ECL3Q.Tests;

/// <summary>
/// Tests for ECL₃^Q algebraic operators.
/// Verifies all truth tables from Handover §2.2.
/// </summary>
public class AlgebraTests
{
    // ── Negation ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(True, False)]
    [InlineData(Undetermined, Undetermined)]
    [InlineData(False, True)]
    public void Negation_MatchesTruthTable(TruthValue input, TruthValue expected)
    {
        Assert.Equal(expected, Operators.Not(input));
    }

    [Fact]
    public void Negation_IsInvolution()
    {
        // ¬¬A = A for all A
        foreach (var v in AllValues())
            Assert.Equal(v, Operators.Not(Operators.Not(v)));
    }

    // ── Conjunction ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(True, True, True)]
    [InlineData(True, Undetermined, Undetermined)]
    [InlineData(True, False, False)]
    [InlineData(Undetermined, True, Undetermined)]
    [InlineData(Undetermined, Undetermined, Undetermined)]
    [InlineData(Undetermined, False, False)]
    [InlineData(False, True, False)]
    [InlineData(False, Undetermined, False)]
    [InlineData(False, False, False)]
    public void Conjunction_MatchesTruthTable(TruthValue a, TruthValue b, TruthValue expected)
    {
        Assert.Equal(expected, Operators.And(a, b));
    }

    [Fact]
    public void Conjunction_IsMin()
    {
        // A ∧ B = min(A,B) in ordering F < U < T
        foreach (var a in AllValues())
            foreach (var b in AllValues())
                Assert.Equal((TruthValue)Math.Min((int)a, (int)b), Operators.And(a, b));
    }

    // ── Disjunction ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(True, True, True)]
    [InlineData(True, Undetermined, True)]
    [InlineData(True, False, True)]
    [InlineData(Undetermined, True, True)]
    [InlineData(Undetermined, Undetermined, Undetermined)]
    [InlineData(Undetermined, False, Undetermined)]
    [InlineData(False, True, True)]
    [InlineData(False, Undetermined, Undetermined)]
    [InlineData(False, False, False)]
    public void Disjunction_MatchesTruthTable(TruthValue a, TruthValue b, TruthValue expected)
    {
        Assert.Equal(expected, Operators.Or(a, b));
    }

    [Fact]
    public void Disjunction_IsMax()
    {
        foreach (var a in AllValues())
            foreach (var b in AllValues())
                Assert.Equal((TruthValue)Math.Max((int)a, (int)b), Operators.Or(a, b));
    }

    // ── Implication ───────────────────────────────────────────────────────────

    [Fact]
    public void Implication_UtoU_IsU_NotT()
    {
        // U→U = U (not T — this differs from classical logic where A→A = T)
        // In K₃: U→U = max(¬U, U) = max(U, U) = U
        Assert.Equal(Undetermined, Operators.Implies(Undetermined, Undetermined));
    }

    [Fact]
    public void Implication_TtoT_IsT()
    {
        Assert.Equal(True, Operators.Implies(True, True));
    }

    [Fact]
    public void Implication_FtoAnything_IsT()
    {
        // F→A = ¬F ∨ A = T ∨ A = T for all A
        foreach (var v in AllValues())
            Assert.Equal(True, Operators.Implies(False, v));
    }

    // ── De Morgan Laws ────────────────────────────────────────────────────────

    [Fact]
    public void DeMorgan_NotAnd()
    {
        // ¬(A∧B) = ¬A∨¬B
        foreach (var a in AllValues())
            foreach (var b in AllValues())
            {
                var left = Operators.Not(Operators.And(a, b));
                var right = Operators.Or(Operators.Not(a), Operators.Not(b));
                Assert.Equal(left, right);
            }
    }

    [Fact]
    public void DeMorgan_NotOr()
    {
        // ¬(A∨B) = ¬A∧¬B
        foreach (var a in AllValues())
            foreach (var b in AllValues())
            {
                var left = Operators.Not(Operators.Or(a, b));
                var right = Operators.And(Operators.Not(a), Operators.Not(b));
                Assert.Equal(left, right);
            }
    }

    // ── Designated Value ──────────────────────────────────────────────────────

    [Fact]
    public void OnlyTrue_IsDesignated()
    {
        Assert.True(Operators.IsDesignated(True));
        Assert.False(Operators.IsDesignated(Undetermined));
        Assert.False(Operators.IsDesignated(False));
    }

    // ── OC Countermodel ───────────────────────────────────────────────────────

    [Fact]
    public void OC_Countermodel_ShowsOCDoesNotHold()
    {
        // OC: obs_w(A∧B) = min(obs_w(A), obs_w(B)) — FALSIFIED in SR_basis
        // for non-standard (joint-witnessing) Obs semantics.
        //
        // The returned values (U, T, T) represent the algebraic countermodel:
        // obs(A)=T, obs(B)=T, but obs(A∧B)=U (joint witnessing fails).
        // Note: the standard EvaluateObs satisfies OC by construction;
        // the countermodel requires a non-standard Obs function.
        // See ObsOperator.GetOcCountermodelValues() for full documentation.
        var (obsConj, obsA, obsB) = ObsOperator.GetOcCountermodelValues();

        // obs_w(A∧B) ≠ min(obs_w(A), obs_w(B)) in the countermodel
        var expectedByOC = (TruthValue)Math.Min((int)obsA, (int)obsB);
        Assert.NotEqual(expectedByOC, obsConj);

        // OC-weak holds: obs_w(A∧B) ≤ min(obs_w(A), obs_w(B))
        Assert.True(ObsOperator.VerifyOcWeak(obsConj, obsA, obsB));
    }

    [Fact]
    public void OCWeak_HoldsForCountermodel()
    {
        var (obsConj, obsA, obsB) = ObsOperator.GetOcCountermodelValues();
        Assert.True(ObsOperator.VerifyOcWeak(obsConj, obsA, obsB));
    }

    [Fact]
    public void Implication_FullTruthTable()
    {
        // Complete implication truth table verification
        var expected = new Dictionary<(TruthValue, TruthValue), TruthValue>
        {
            { (True,  True),  True  },
            { (True,  Undetermined), Undetermined },
            { (True,  False), False },
            { (Undetermined, True),  True  },
            { (Undetermined, Undetermined), Undetermined },
            { (Undetermined, False), Undetermined },
            { (False, True),  True  },
            { (False, Undetermined), True  },
            { (False, False), True  },
        };
        foreach (var ((a, b), exp) in expected)
            Assert.Equal(exp, Operators.Implies(a, b));
    }

    [Fact]
    public void Biconditional_UtoU_IsU()
    {
        // U↔U = min(U→U, U→U) = min(U, U) = U (not T)
        Assert.Equal(Undetermined, Operators.Iff(Undetermined, Undetermined));
    }

    [Fact]
    public void Biconditional_TtoT_IsT()
    {
        Assert.Equal(True, Operators.Iff(True, True));
    }

    [Fact]
    public void Biconditional_FtoF_IsT()
    {
        Assert.Equal(True, Operators.Iff(False, False));
    }

    [Fact]
    public void OC_StandardEvaluateObs_SatisfiesOC()
    {
        // With the standard EvaluateObs semantics, OC holds by construction.
        // If obs(A)=T and obs(B)=T, then all successors agree on A=T and B=T,
        // so A∧B=T at all successors → obs(A∧B)=T = min(T,T).
        // This test documents this property of the standard implementation.
        var w  = new World("w");
        var v1 = new World("v1");
        var v2 = new World("v2");
        var frame = new Frame(
            worlds: [w, v1, v2],
            rObs: [(w, v1), (w, v2)]);
        var val = new Valuation([
            (("a", w),  TruthValue.True),
            (("a", v1), TruthValue.True),
            (("a", v2), TruthValue.True),
            (("b", w),  TruthValue.True),
            (("b", v1), TruthValue.True),
            (("b", v2), TruthValue.True),
        ]);
        var model = new Model(frame, val);
        var a = new Atom("a");
        var b = new Atom("b");
        var aAndB = new Conjunction(a, b);

        var obsA    = model.Evaluate(new ObsFormula(a),    w);
        var obsB    = model.Evaluate(new ObsFormula(b),    w);
        var obsAandB = model.Evaluate(new ObsFormula(aAndB), w);

        // Standard EvaluateObs satisfies OC here: obs(A∧B) = min(obs(A), obs(B))
        var minObs = (TruthValue)Math.Min((int)obsA, (int)obsB);
        Assert.Equal(minObs, obsAandB);
        // All three are T
        Assert.Equal(True, obsA);
        Assert.Equal(True, obsB);
        Assert.Equal(True, obsAandB);
    }

    private static IEnumerable<TruthValue> AllValues() =>
        [TruthValue.False, TruthValue.Undetermined, TruthValue.True];
}
