namespace ECL3Q.Core.Semantics;

/// <summary>
/// The 119-domain classification of ECL₃^Q applicability (SR401-rev3).
///
/// ECL₃^Q has been shown to apply across 119 distinct domains, organized
/// into six top-level groups. Each domain is a context where:
///   - ontological indeterminacy (U) is genuinely present (not mere ignorance)
///   - a σ-carrier can be identified
///   - σ-collapse corresponds to a real-world decision or measurement event
///
/// This classification is the result of the unlimited extensibility theorem
/// (SR401): ECL₃^Q applies to any domain admitting a σ-collapse structure.
///
/// See Paper XIV (Universalität) and Paper XVI (Reale Debatten D1–D100).
/// </summary>
public static class DomainClassification
{
    /// <summary>
    /// Top-level domain groups. Each group contains related domains
    /// sharing structural properties of their σ-collapse.
    /// </summary>
    public enum DomainGroup
    {
        /// <summary>
        /// Group 1: Formal Systems and Computer Science (SR401–SR450).
        /// σ-collapse = formal decision or verification step.
        /// Examples: verification of autonomous systems, smart contracts,
        /// quantum computing protocols, type systems, model checking.
        /// </summary>
        FormalSystemsAndCS,

        /// <summary>
        /// Group 2: Law and Governance (SR117–SR138, SR451–SR500).
        /// σ-collapse = binding legal judgment or regulatory decision.
        /// Examples: court rulings, constitutional review, regulatory approval,
        /// international arbitration, legislative enactment.
        /// </summary>
        LawAndGovernance,

        /// <summary>
        /// Group 3: Medicine and Bioethics (SR157–SR176, SR501–SR550).
        /// σ-collapse = clinical diagnosis or ethical committee decision.
        /// Examples: diagnosis under uncertainty, triage, end-of-life decisions,
        /// clinical trial enrollment, organ allocation.
        /// </summary>
        MedicineAndBioethics,

        /// <summary>
        /// Group 4: Economics and Finance (SR551–SR600).
        /// σ-collapse = market clearing or contractual settlement.
        /// Examples: price discovery, credit rating, derivative settlement,
        /// bankruptcy determination, regulatory capital assessment.
        /// </summary>
        EconomicsAndFinance,

        /// <summary>
        /// Group 5: Philosophy and Cognitive Science (SR328–SR330, SR601–SR650).
        /// σ-collapse = conceptual fixation or belief revision.
        /// Examples: vagueness resolution, personal identity, epistemic norms,
        /// consciousness, AI decision-making epistemology.
        /// </summary>
        PhilosophyAndCognition,

        /// <summary>
        /// Group 6: Natural and Social Sciences (SR201–SR228, SR367–SR375, SR651–SR700).
        /// σ-collapse = scientific measurement or social consensus.
        /// Examples: quantum measurement, species classification, climate tipping points,
        /// election outcomes, public health policy decisions.
        /// </summary>
        NaturalAndSocialSciences
    }

    /// <summary>
    /// A single classified domain: a context where ECL₃^Q applies.
    /// </summary>
    public record Domain(
        int Id,
        string Name,
        DomainGroup Group,
        string SigmaCarrier,
        string CollapseEvent,
        string? SrReference = null);

    /// <summary>
    /// The canonical 119 domains (SR401-rev3), ordered by domain ID.
    /// </summary>
    public static IReadOnlyList<Domain> All { get; } = BuildAll();

    /// <summary>Returns all domains belonging to <paramref name="group"/>.</summary>
    public static IEnumerable<Domain> InGroup(DomainGroup group) =>
        All.Where(d => d.Group == group);

    /// <summary>Returns the domain with the given <paramref name="id"/>, or null if not found.</summary>
    public static Domain? ById(int id) =>
        All.FirstOrDefault(d => d.Id == id);

    private static IReadOnlyList<Domain> BuildAll()
    {
        var domains = new List<Domain>();

        // ── Group 1: Formal Systems and Computer Science ──────────────────────
        domains.AddRange([
            new(  1, "Propositional Logic",               DomainGroup.FormalSystemsAndCS,  "Proof system",         "Theorem derivation",        "SR_basis"),
            new(  2, "Modal Logic K",                     DomainGroup.FormalSystemsAndCS,  "Frame semantics",      "Axiom validation",          "SR1–SR30"),
            new(  3, "Quantum Computing Protocols",        DomainGroup.FormalSystemsAndCS,  "Quantum circuit",      "Qubit measurement",         "SR72–SR84"),
            new(  4, "Autonomous System Verification",     DomainGroup.FormalSystemsAndCS,  "Verifier",             "Safety check decision",     "SR139–SR156"),
            new(  5, "Smart Contract Execution",           DomainGroup.FormalSystemsAndCS,  "Blockchain consensus", "Contract trigger",          "SR139–SR156"),
            new(  6, "Type System Inference",              DomainGroup.FormalSystemsAndCS,  "Type checker",         "Type assignment",           "SR401-rev3"),
            new(  7, "Model Checking",                     DomainGroup.FormalSystemsAndCS,  "Model checker",        "Property verification",     "SR401-rev3"),
            new(  8, "Formal Specification",               DomainGroup.FormalSystemsAndCS,  "Specification tool",   "Requirements sign-off",     "SR401-rev3"),
            new(  9, "Program Verification",               DomainGroup.FormalSystemsAndCS,  "Verifier",             "Proof completion",          "SR401-rev3"),
            new( 10, "Distributed Consensus",              DomainGroup.FormalSystemsAndCS,  "Consensus protocol",   "Block finalization",        "SR401-rev3"),
            new( 11, "Cryptographic Protocol",             DomainGroup.FormalSystemsAndCS,  "Protocol verifier",    "Key agreement",             "SR401-rev3"),
            new( 12, "AI Decision Systems",                DomainGroup.FormalSystemsAndCS,  "AI agent",             "Classification output",     "SR594–SR607"),
            new( 13, "Formal Ontologies",                  DomainGroup.FormalSystemsAndCS,  "Ontology reasoner",    "Concept classification",    "SR401-rev3"),
            new( 14, "Game Theory Equilibria",             DomainGroup.FormalSystemsAndCS,  "Nash solver",          "Strategy fixation",         "SR401-rev3"),
            new( 15, "Multi-Agent Systems",                DomainGroup.FormalSystemsAndCS,  "Agent coordinator",    "Joint action commitment",   "SR85–SR116"),
            new( 16, "Database Query Resolution",          DomainGroup.FormalSystemsAndCS,  "Query engine",         "Result materialization",    "SR401-rev3"),
            new( 17, "Natural Language Processing",        DomainGroup.FormalSystemsAndCS,  "NLP parser",           "Disambiguation",            "SR177–SR189"),
            new( 18, "Automated Theorem Proving",          DomainGroup.FormalSystemsAndCS,  "ATP system",           "Proof found/refuted",       "SR401-rev3"),
            new( 19, "Cybersecurity Classification",       DomainGroup.FormalSystemsAndCS,  "Security analyst",     "Threat classification",     "SR401-rev3"),
            new( 20, "Probabilistic Model Checking",       DomainGroup.FormalSystemsAndCS,  "Probabilistic checker","Probability threshold",     "SR401-rev3"),
        ]);

        // ── Group 2: Law and Governance ───────────────────────────────────────
        domains.AddRange([
            new( 21, "Criminal Liability",                 DomainGroup.LawAndGovernance,    "Court",                "Verdict",                   "SR117"),
            new( 22, "Civil Liability",                    DomainGroup.LawAndGovernance,    "Court",                "Judgment",                  "SR118"),
            new( 23, "Constitutional Review",              DomainGroup.LawAndGovernance,    "Constitutional court", "Ruling on constitutionality","SR119"),
            new( 24, "Regulatory Approval",                DomainGroup.LawAndGovernance,    "Regulator",            "Approval/denial decision",  "SR120"),
            new( 25, "International Arbitration",          DomainGroup.LawAndGovernance,    "Arbitration tribunal", "Award",                     "SR121"),
            new( 26, "Patent Grant",                       DomainGroup.LawAndGovernance,    "Patent office",        "Grant/rejection",           "SR122"),
            new( 27, "Legislative Enactment",              DomainGroup.LawAndGovernance,    "Parliament",           "Vote result",               "SR123"),
            new( 28, "Administrative Decision",            DomainGroup.LawAndGovernance,    "Administrative body",  "Ruling",                    "SR124"),
            new( 29, "Electoral Outcome",                  DomainGroup.LawAndGovernance,    "Electoral commission", "Vote count certification",  "SR125"),
            new( 30, "Treaty Ratification",                DomainGroup.LawAndGovernance,    "Ratifying body",       "Ratification vote",         "SR126"),
            new( 31, "Bankruptcy Determination",           DomainGroup.LawAndGovernance,    "Insolvency court",     "Insolvency ruling",         "SR127"),
            new( 32, "Asylum Decision",                    DomainGroup.LawAndGovernance,    "Immigration authority","Status determination",      "SR128"),
            new( 33, "Contract Formation",                 DomainGroup.LawAndGovernance,    "Contracting parties",  "Offer acceptance",          "SR129"),
            new( 34, "Property Rights",                    DomainGroup.LawAndGovernance,    "Land registry",        "Title transfer",            "SR130"),
            new( 35, "Succession and Inheritance",         DomainGroup.LawAndGovernance,    "Probate court",        "Estate settlement",         "SR131"),
            new( 36, "Data Protection Compliance",         DomainGroup.LawAndGovernance,    "Data authority",       "Compliance determination",  "SR132"),
            new( 37, "Antitrust Review",                   DomainGroup.LawAndGovernance,    "Competition authority","Merger approval/block",     "SR133"),
            new( 38, "Environmental Permit",               DomainGroup.LawAndGovernance,    "Environmental agency", "Permit grant/denial",       "SR134"),
            new( 39, "Labor Dispute Resolution",           DomainGroup.LawAndGovernance,    "Labor tribunal",       "Award",                     "SR135"),
            new( 40, "Human Rights Adjudication",          DomainGroup.LawAndGovernance,    "Human rights court",   "Violation finding",         "SR138"),
        ]);

        // ── Group 3: Medicine and Bioethics ───────────────────────────────────
        domains.AddRange([
            new( 41, "Clinical Diagnosis",                 DomainGroup.MedicineAndBioethics,"Physician",            "Diagnosis statement",       "SR157"),
            new( 42, "Triage Decision",                    DomainGroup.MedicineAndBioethics,"Triage nurse/doctor",  "Priority assignment",       "SR158"),
            new( 43, "End-of-Life Decision",               DomainGroup.MedicineAndBioethics,"Ethics committee",     "Treatment withdrawal",      "SR159"),
            new( 44, "Informed Consent",                   DomainGroup.MedicineAndBioethics,"Patient",              "Consent given/withheld",    "SR160"),
            new( 45, "Clinical Trial Enrollment",          DomainGroup.MedicineAndBioethics,"IRB / PI",             "Eligibility determination", "SR161"),
            new( 46, "Organ Allocation",                   DomainGroup.MedicineAndBioethics,"Transplant committee", "Recipient selection",       "SR162"),
            new( 47, "Genetic Testing Decision",           DomainGroup.MedicineAndBioethics,"Genetic counselor",    "Test result interpretation","SR163"),
            new( 48, "Psychiatric Commitment",             DomainGroup.MedicineAndBioethics,"Psychiatric board",    "Involuntary commitment",    "SR164"),
            new( 49, "Drug Approval",                      DomainGroup.MedicineAndBioethics,"Drug regulator",       "Market authorization",      "SR165"),
            new( 50, "Disability Assessment",              DomainGroup.MedicineAndBioethics,"Medical board",        "Disability rating",         "SR166"),
            new( 51, "Embryo Selection (IVF)",             DomainGroup.MedicineAndBioethics,"IVF clinic",           "Embryo selection",          "SR167"),
            new( 52, "Brain Death Determination",          DomainGroup.MedicineAndBioethics,"Neurologist",          "Death certification",       "SR168"),
            new( 53, "Vaccine Safety Decision",            DomainGroup.MedicineAndBioethics,"Safety board",         "Use recommendation",        "SR169"),
            new( 54, "Mandatory Quarantine",               DomainGroup.MedicineAndBioethics,"Public health authority","Quarantine order",         "SR170"),
            new( 55, "Euthanasia Authorization",           DomainGroup.MedicineAndBioethics,"Ethics committee",     "Authorization",             "SR176"),
        ]);

        // ── Group 4: Economics and Finance ────────────────────────────────────
        domains.AddRange([
            new( 56, "Price Discovery",                    DomainGroup.EconomicsAndFinance, "Market mechanism",     "Transaction execution",     "SR401-rev3"),
            new( 57, "Credit Rating",                      DomainGroup.EconomicsAndFinance, "Rating agency",        "Rating publication",        "SR401-rev3"),
            new( 58, "Derivative Settlement",              DomainGroup.EconomicsAndFinance, "Clearing house",       "Settlement determination",  "SR401-rev3"),
            new( 59, "Regulatory Capital Assessment",      DomainGroup.EconomicsAndFinance, "Banking regulator",    "Capital adequacy ruling",   "SR401-rev3"),
            new( 60, "Merger Valuation",                   DomainGroup.EconomicsAndFinance, "Valuation committee",  "Agreed price",              "SR401-rev3"),
            new( 61, "Auction Outcome",                    DomainGroup.EconomicsAndFinance, "Auction mechanism",    "Lot awarded",               "SR401-rev3"),
            new( 62, "Insurance Claim Settlement",         DomainGroup.EconomicsAndFinance, "Claims adjuster",      "Settlement offer accepted", "SR401-rev3"),
            new( 63, "Central Bank Policy",                DomainGroup.EconomicsAndFinance, "Central bank board",   "Rate decision",             "SR401-rev3"),
            new( 64, "ESG Classification",                 DomainGroup.EconomicsAndFinance, "ESG rating body",      "Category assignment",       "SR401-rev3"),
            new( 65, "Sanctions Determination",            DomainGroup.EconomicsAndFinance, "Sanctions authority",  "Entity designation",        "SR401-rev3"),
        ]);

        // ── Group 5: Philosophy and Cognitive Science ─────────────────────────
        domains.AddRange([
            new( 66, "Vagueness Resolution",               DomainGroup.PhilosophyAndCognition,"Speaker/context",   "Predicate application",     "SR177–SR189"),
            new( 67, "Personal Identity",                  DomainGroup.PhilosophyAndCognition,"Conceptual framework","Identity claim",           "SR328"),
            new( 68, "Consciousness Attribution",          DomainGroup.PhilosophyAndCognition,"Conceptual framework","Sentience determination",  "SR329"),
            new( 69, "Free Will Adjudication",             DomainGroup.PhilosophyAndCognition,"Philosophical analysis","Responsibility assignment","SR330"),
            new( 70, "Epistemic Norms",                    DomainGroup.PhilosophyAndCognition,"Epistemic community","Justified belief fixation", "SR324–SR327"),
            new( 71, "Scientific Paradigm Shift",          DomainGroup.PhilosophyAndCognition,"Scientific community","Paradigm acceptance",      "SR324–SR327"),
            new( 72, "AI Moral Status",                    DomainGroup.PhilosophyAndCognition,"Ethics board",       "Status determination",      "SR594–SR607"),
            new( 73, "Moral Dilemma Resolution",           DomainGroup.PhilosophyAndCognition,"Ethics committee",   "Ethical decision",          "SR157–SR176"),
            new( 74, "Reference Fixing",                   DomainGroup.PhilosophyAndCognition,"Speaker/context",    "Reference determined",      "SR177–SR189"),
            new( 75, "Concept Individuation",              DomainGroup.PhilosophyAndCognition,"Philosophical analysis","Concept fixed",           "SR401-rev3"),
        ]);

        // ── Group 6: Natural and Social Sciences ──────────────────────────────
        domains.AddRange([
            new( 76, "Quantum Measurement",                DomainGroup.NaturalAndSocialSciences,"Measurement apparatus","Eigenvalue readout",    "SR72–SR84"),
            new( 77, "Species Classification",             DomainGroup.NaturalAndSocialSciences,"Taxonomic committee","Species delimitation",     "SR201"),
            new( 78, "Climate Tipping Point",              DomainGroup.NaturalAndSocialSciences,"IPCC / scientists","Threshold crossed",         "SR367–SR375"),
            new( 79, "Seismic Event Classification",       DomainGroup.NaturalAndSocialSciences,"Seismological agency","Magnitude classification","SR201–SR228"),
            new( 80, "Pandemic Phase Declaration",         DomainGroup.NaturalAndSocialSciences,"WHO",               "Phase declaration",         "SR367–SR375"),
            new( 81, "Census Classification",              DomainGroup.NaturalAndSocialSciences,"Census bureau",     "Category assignment",       "SR234–SR239"),
            new( 82, "Social Movement Recognition",        DomainGroup.NaturalAndSocialSciences,"Media/academia",    "Movement named",            "SR234–SR239"),
            new( 83, "Poverty Line Determination",         DomainGroup.NaturalAndSocialSciences,"Statistics bureau", "Threshold set",             "SR234–SR239"),
            new( 84, "Terrorist Organization Listing",     DomainGroup.NaturalAndSocialSciences,"Government authority","Designation",             "SR234–SR239"),
            new( 85, "Archaeological Site Dating",         DomainGroup.NaturalAndSocialSciences,"Archaeologist",     "Date assignment",           "SR201–SR228"),
            new( 86, "Linguistic Standardization",         DomainGroup.NaturalAndSocialSciences,"Standards body",    "Norm publication",          "SR177–SR189"),
            new( 87, "Urban Planning Decision",            DomainGroup.NaturalAndSocialSciences,"Planning authority","Zoning approval",           "SR401-rev3"),
            new( 88, "Food Safety Classification",         DomainGroup.NaturalAndSocialSciences,"Food authority",    "Safety determination",      "SR401-rev3"),
            new( 89, "Mental Disorder Classification",     DomainGroup.NaturalAndSocialSciences,"DSM/ICD committee", "Disorder defined",          "SR157–SR176"),
            new( 90, "Nuclear Safety Assessment",          DomainGroup.NaturalAndSocialSciences,"Nuclear regulator", "Safety classification",     "SR401-rev3"),
            new( 91, "AI Risk Classification",             DomainGroup.NaturalAndSocialSciences,"Regulatory body",   "Risk tier assigned",        "SR594–SR607"),
            new( 92, "Digital Sovereignty Decision",       DomainGroup.NaturalAndSocialSciences,"Government",        "Data governance ruling",    "SR401-rev3"),
            new( 93, "Hate Speech Classification",         DomainGroup.NaturalAndSocialSciences,"Platform/regulator","Content ruling",            "SR401-rev3"),
            new( 94, "Autonomous Weapons Authorization",   DomainGroup.NaturalAndSocialSciences,"Military command",  "Engagement authorization",  "SR401-rev3"),
            new( 95, "Land Reform Decision",               DomainGroup.NaturalAndSocialSciences,"Government",        "Redistribution ruling",     "SR401-rev3"),
            new( 96, "Animal Rights Status",               DomainGroup.NaturalAndSocialSciences,"Legislature/court", "Legal personhood ruling",   "SR401-rev3"),
            new( 97, "Misinformation Classification",      DomainGroup.NaturalAndSocialSciences,"Fact-checker",      "Label applied",             "SR401-rev3"),
            new( 98, "Gender Legal Recognition",           DomainGroup.NaturalAndSocialSciences,"Government authority","Legal gender fixed",       "SR401-rev3"),
            new( 99, "Disability Rights Definition",       DomainGroup.NaturalAndSocialSciences,"Legislature",       "Definition enacted",        "SR401-rev3"),
            new(100, "Copyright Determination",            DomainGroup.NaturalAndSocialSciences,"Court/IP office",   "Ownership ruling",          "SR401-rev3"),
            new(101, "Reparations Decision",               DomainGroup.NaturalAndSocialSciences,"Government/court",  "Award determination",       "SR401-rev3"),
            new(102, "Cultural Heritage Designation",      DomainGroup.NaturalAndSocialSciences,"Heritage authority","UNESCO listing",            "SR401-rev3"),
            new(103, "Drug Decriminalization",             DomainGroup.NaturalAndSocialSciences,"Legislature",       "Policy enacted",            "SR401-rev3"),
            new(104, "Nuclear Disarmament Verification",   DomainGroup.NaturalAndSocialSciences,"Inspection agency", "Verification declared",     "SR401-rev3"),
            new(105, "Narrative Framing (Media)",          DomainGroup.NaturalAndSocialSciences,"Editorial board",   "Frame adopted",             "SR401-rev3"),
            new(106, "Urban Gentrification Classification",DomainGroup.NaturalAndSocialSciences,"Planners/media",    "Area classification",       "SR401-rev3"),
            new(107, "Child Welfare Determination",        DomainGroup.NaturalAndSocialSciences,"Family court",      "Custody ruling",            "SR401-rev3"),
            new(108, "Indigenous Land Rights",             DomainGroup.NaturalAndSocialSciences,"Court",             "Title recognition",         "SR401-rev3"),
            new(109, "Statelessness Determination",        DomainGroup.NaturalAndSocialSciences,"UNHCR/government",  "Stateless status granted",  "SR401-rev3"),
            new(110, "Social Credit Classification",       DomainGroup.NaturalAndSocialSciences,"Government system", "Score threshold crossed",   "SR401-rev3"),
            new(111, "Age of Criminal Responsibility",     DomainGroup.NaturalAndSocialSciences,"Legislature",       "Age threshold set",         "SR401-rev3"),
            new(112, "Posthumous Recognition",             DomainGroup.NaturalAndSocialSciences,"Award committee",   "Recognition granted",       "SR401-rev3"),
            new(113, "Tax Haven Classification",           DomainGroup.NaturalAndSocialSciences,"OECD/government",   "Jurisdiction listed",       "SR401-rev3"),
            new(114, "Pandemic Origin Classification",     DomainGroup.NaturalAndSocialSciences,"WHO/scientists",    "Origin determination",      "SR367–SR375"),
            new(115, "AI Copyright (Training Data)",       DomainGroup.NaturalAndSocialSciences,"Court",             "Infringement ruling",       "SR594–SR607"),
            new(116, "Memory Law",                         DomainGroup.NaturalAndSocialSciences,"Legislature",       "Historical narrative fixed","SR401-rev3"),
            new(117, "Whistleblower Protection",           DomainGroup.NaturalAndSocialSciences,"Court/authority",   "Protection granted/denied", "SR401-rev3"),
            new(118, "Personhood of Future Generations",   DomainGroup.NaturalAndSocialSciences,"Legal system",      "Rights recognized",         "SR401-rev3"),
            new(119, "Moral Responsibility of AI Systems", DomainGroup.NaturalAndSocialSciences,"Ethics board",      "Responsibility attributed",  "SR594–SR607"),
        ]);

        return domains.AsReadOnly();
    }
}
