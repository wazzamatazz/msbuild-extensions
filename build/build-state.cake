// Class for sharing build state between Cake tasks.
public class BuildState {

    // The solution to build.
    public string SolutionName { get; set; }

    // The Cake target.
    public string Target { get; set; }

    // The MSBuild configuration.
    public string Configuration { get; set; }

    // Specifies if a clean should be performed prior to running the specified target.
    public bool Clean { get; set; }

    // Specifies if the Clean target should be run.
    public bool RunCleanTarget => Clean || string.Equals(Target, "Clean", StringComparison.OrdinalIgnoreCase);

    // Specifies if this is a continuous integration build.
    public bool ContinuousIntegrationBuild { get; set; }

    // Specifies if verbose logging should be used.
    public bool Verbose { get; set; }

}
