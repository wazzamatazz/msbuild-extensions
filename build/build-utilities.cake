// Miscellaneous build utilities.
public static class BuildUtilities {

    // Writes a log message.
    public static void WriteLogMessage(BuildSystem buildSystem, string message) {
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.WriteProgressMessage(message);
        }
        else {
            Console.WriteLine(message);
        }
    }


    // Writes a task started message.
    public static void WriteTaskStartMessage(BuildSystem buildSystem, string description) {
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.WriteStartBuildBlock(description);
        }
    }


    // Writes a task completed message.
    public static void WriteTaskEndMessage(BuildSystem buildSystem, string description) {
        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.WriteEndBuildBlock(description);
        }
    }


    // Writes the specified build state to the log.
    public static void WriteBuildStateToLog(BuildSystem buildSystem, BuildState state) {
        WriteLogMessage(buildSystem, $"Solution Name: {state.SolutionName}");
        WriteLogMessage(buildSystem, $"Target: {state.Target}");
        WriteLogMessage(buildSystem, $"Configuration: {state.Configuration}");
        WriteLogMessage(buildSystem, $"Clean: {state.RunCleanTarget}");
        WriteLogMessage(buildSystem, $"Continuous Integration Build: {state.ContinuousIntegrationBuild}");
    }


    // Adds MSBuild properties from the build state.
    public static void ApplyMSBuildProperties(DotNetCoreMSBuildSettings settings, BuildState state) {
        // Specify if this is a CI build. 
        if (state.ContinuousIntegrationBuild) {
            settings.Properties["ContinuousIntegrationBuild"] = new List<string> { "True" };
        }
    }


    // Imports test results into the build system.
    public static void ImportTestResults(BuildSystem buildSystem, string testProvider, FilePath resultsFile) {
        if (resultsFile == null) {
            return;
        }

        if (buildSystem.IsRunningOnTeamCity) {
            buildSystem.TeamCity.ImportData(testProvider, resultsFile);
        }
    }

}
