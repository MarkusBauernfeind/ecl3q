using ECL3Q.Core.Inference;
using ECL3Q.Core.Syntax;
using Xunit;
using static ECL3Q.Core.Syntax.F;

namespace ECL3Q.Tests;

/// <summary>
/// Tests for the proof search / tableau system.
/// </summary>
public class InferenceTests
{
    [Fact]
    public void ClassicalTautology_ModusPonens_IsValid()
    {
        // (p ∧ (p→q)) → q is classically valid
        var mp = Implies(And(P, Implies(P, Q)), Q);
        Assert.True(ProofSearch.IsClassicalTautology(mp));
    }

    [Fact]
    public void ClassicalTautology_HypotheticalSyllogism_IsValid()
    {
        // (p→q) ∧ (q→r) → (p→r)
        var hs = Implies(And(Implies(P, Q), Implies(Q, R)), Implies(P, R));
        Assert.True(ProofSearch.IsClassicalTautology(hs));
    }

    [Fact]
    public void ClassicalTautology_ConjunctionElimination_IsValid()
    {
        // p ∧ q → p: classically valid but NOT K₃ (p=U,q=U → U∧U=U → U→U=U ≠ T)
        Assert.True(ProofSearch.IsClassicalTautology(Implies(And(P, Q), P)));
        Assert.False(ProofSearch.IsK3Tautology(Implies(And(P, Q), P)));
    }

    [Fact]
    public void ClassicalTautology_DisjunctionIntroduction_IsValid()
    {
        // p → p ∨ q: classically valid but NOT K₃ (p=U,q=F → U→max(U,F)=U→U=U ≠ T)
        Assert.True(ProofSearch.IsClassicalTautology(Implies(P, Or(P, Q))));
        Assert.False(ProofSearch.IsK3Tautology(Implies(P, Or(P, Q))));
    }

    [Fact]
    public void LEM_NotK3Valid_IsK3Valid_IsClassicalValid()
    {
        var lem = Or(P, Not(P));
        Assert.False(ProofSearch.IsK3Tautology(lem));
        Assert.True(ProofSearch.IsClassicalTautology(lem));
    }

    [Fact]
    public void NonContradiction_NotK3Valid()
    {
        // ¬(p ∧ ¬p): K₃ says U∧¬U = U∧U = U, ¬U = U ≠ T
        var nc = Not(And(P, Not(P)));
        Assert.False(ProofSearch.IsK3Tautology(nc));
        Assert.True(ProofSearch.IsClassicalTautology(nc));
    }

    [Fact]
    public void Tableau_IsValid_pImpliesp_IsClassicallyValid()
    {
        Assert.True(Tableau.IsValid(Implies(P, P)));
    }

    [Fact]
    public void Tableau_IsValid_LEM_IsClassicallyValid()
    {
        Assert.True(Tableau.IsValid(Or(P, Not(P))));
    }

    [Fact]
    public void Tableau_IsValid_NonContradiction_IsClassicallyValid()
    {
        Assert.True(Tableau.IsValid(Not(And(P, Not(P)))));
    }

    [Fact]
    public void Tableau_IsValid_DoubleNegation_IsClassicallyValid()
    {
        Assert.True(Tableau.IsValid(Iff(Not(Not(P)), P)));
    }

    [Fact]
    public void Tableau_IsValid_ConjunctionElim_IsClassicallyValid()
    {
        Assert.True(Tableau.IsValid(Implies(And(P, Q), P)));
    }

    [Fact]
    public void Tableau_IsValid_pImpliesqImpliesq_IsNotValid()
    {
        // (p→q)→q is NOT classically valid: p=F, q=F → (F→F)→F = T→F = F
        Assert.False(Tableau.IsValid(Implies(Implies(P, Q), Q)));
    }

    [Fact]
    public void Tableau_IsValid_K3Check_pImpliesp_NotK3()
    {
        // p→p: classically valid, NOT K₃ (p=U → U→U=U)
        Assert.False(ProofSearch.IsK3Tautology(Implies(P, P)));
        Assert.True(ProofSearch.IsClassicalTautology(Implies(P, P)));
    }

    [Fact]
    public void Tableau_IsValid_SoundnessCheck_ImplicationNotTautology()
    {
        // Regression: old Tableau.IsValid had a soundness bug where T:(A→B) only
        // added T:B (not branching on F:A | T:B), causing false positives.
        // (p→q)→q is NOT classically valid.
        // p=F, q=F: (F→F)→F = T→F = F.
        var formula = Implies(Implies(P, Q), Q);
        Assert.False(Tableau.IsValid(formula), "(p→q)→q is not a tautology");
        Assert.False(ProofSearch.IsClassicalTautology(formula));
    }

    [Fact]
    public void K3ClassicalEquivalence_WhenNoU()
    {
        // Verify: K₃ tautologies are a subset of classical tautologies? No — depends on formula.
        // But: any K₃ tautology is also classically valid (K₃ restricted to {T,F} = classical).
        var formulas = new Formula[]
        {
            Implies(And(P, Q), P),
            Implies(P, Or(P, Q)),
            Iff(Not(Not(P)), P)
        };

        foreach (var f in formulas)
        {
            if (ProofSearch.IsK3Tautology(f))
                Assert.True(ProofSearch.IsClassicalTautology(f),
                    $"K₃ tautology {f} should also be classically valid");
        }
    }
}
