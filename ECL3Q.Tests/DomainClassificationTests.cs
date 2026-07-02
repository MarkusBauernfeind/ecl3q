using ECL3Q.Core.Semantics;
using Xunit;

namespace ECL3Q.Tests;

public class DomainClassificationTests
{
    [Fact]
    public void DomainCount_IsExactly119()
    {
        Assert.Equal(119, DomainClassification.All.Count);
    }

    [Fact]
    public void DomainIds_AreUnique()
    {
        var ids = DomainClassification.All.Select(d => d.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void DomainIds_AreSequential_1_To_119()
    {
        var ids = DomainClassification.All.Select(d => d.Id).OrderBy(x => x).ToList();
        Assert.Equal(Enumerable.Range(1, 119).ToList(), ids);
    }

    [Fact]
    public void AllSixGroups_AreRepresented()
    {
        var groups = DomainClassification.All.Select(d => d.Group).Distinct().ToHashSet();
        foreach (var group in Enum.GetValues<DomainClassification.DomainGroup>())
            Assert.Contains(group, groups);
    }

    [Fact]
    public void EachDomain_HasNonEmptySigmaCarrierAndCollapseEvent()
    {
        foreach (var d in DomainClassification.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(d.SigmaCarrier),
                $"Domain {d.Id} '{d.Name}' has empty SigmaCarrier");
            Assert.False(string.IsNullOrWhiteSpace(d.CollapseEvent),
                $"Domain {d.Id} '{d.Name}' has empty CollapseEvent");
        }
    }

    [Fact]
    public void ById_ReturnsCorrectDomain()
    {
        var d1 = DomainClassification.ById(1);
        Assert.NotNull(d1);
        Assert.Equal("Propositional Logic", d1.Name);

        var d119 = DomainClassification.ById(119);
        Assert.NotNull(d119);
        Assert.Equal("Moral Responsibility of AI Systems", d119.Name);

        Assert.Null(DomainClassification.ById(0));
        Assert.Null(DomainClassification.ById(120));
    }

    [Fact]
    public void InGroup_FormalCS_ContainsAtLeast10Domains()
    {
        var count = DomainClassification.InGroup(
            DomainClassification.DomainGroup.FormalSystemsAndCS).Count();
        Assert.True(count >= 10);
    }

    [Fact]
    public void LawDomains_AllHaveCourtOrAuthority()
    {
        foreach (var d in DomainClassification.InGroup(
                     DomainClassification.DomainGroup.LawAndGovernance))
        {
            // σ-carrier in law domain should be an institutional body
            Assert.False(string.IsNullOrWhiteSpace(d.SigmaCarrier));
        }
    }

    [Fact]
    public void Domain76_IsQuantumMeasurement()
    {
        // SR72–SR84 quantum extension — domain 76 should reference it
        var qm = DomainClassification.ById(76);
        Assert.NotNull(qm);
        Assert.Equal(DomainClassification.DomainGroup.NaturalAndSocialSciences, qm.Group);
        Assert.Contains("SR72", qm.SrReference ?? "");
    }
}
