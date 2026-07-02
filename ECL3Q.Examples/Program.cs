namespace ECL3Q.Examples;

/// <summary>
/// ECL₃^Q example programs.
/// Run with: dotnet run --project ECL3Q.Examples [example-number]
///
/// Available examples:
///   1  — Quantum collapse: particle spin in superposition, σ-collapse to eigenstate
///   2  — Legal judgment: court decisions as σ-collapse, σ-conflict detection
///   3  — Tableau boundaries: complete fragments and open problem C (U:Obs Source 2)
///   all — run all examples
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        var which = args.Length > 0 ? args[0] : "all";

        Console.WriteLine();
        Console.WriteLine("ECL₃^Q Examples");
        Console.WriteLine("Epistemic Causal Logic, Three-Valued, Quantum Extension");
        Console.WriteLine("Markus Bauernfeind | AI-assisted by Claude (Anthropic)");
        Console.WriteLine();

        switch (which)
        {
            case "1":
                Example1_QuantumCollapse.Run();
                break;
            case "2":
                Example2_LegalJudgment.Run();
                break;
            case "3":
                Example3_TableauBoundaries.Run();
                break;
            case "all":
                Example1_QuantumCollapse.Run();
                Console.WriteLine();
                Example2_LegalJudgment.Run();
                Console.WriteLine();
                Example3_TableauBoundaries.Run();
                break;
            default:
                Console.WriteLine($"Unknown example '{which}'. Use: 1, 2, 3, or all.");
                break;
        }
    }
}
