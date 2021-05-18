const string DefaultSolutionName = "./Jaahas.MSBuild.Extensions.sln";

///////////////////////////////////////////////////////////////////////////////////////////////////
// COMMAND LINE ARGUMENTS:
//
// --project=<PROJECT OR SOLUTION>
//   The MSBuild project or solution to build. 
//     Default: see DefaultSolutionName constant above.
//
// --target=<TARGET>
//   Specifies the Cake target to run. 
//     Default: Test
//     Possible Values: Clean, Restore, Build, Pack
//
// --configuration=<CONFIGURATION>
//   Specifies the MSBuild configuration to use. 
//     Default: Debug
//
// --clean
//   Specifies if this is a rebuild rather than an incremental build. All artifact and bin folders 
//   will be cleaned prior to running the specified target.
//
// --ci
//   Forces continuous integration build mode. Not required if the build is being run by a 
//   supported continuous integration build system.
//
// --verbose
//   Enables verbose messages.
// 
///////////////////////////////////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.Git&version=1.0.0

#load "build/build-state.cake"
#load "build/build-utilities.cake"

// Get the target that was specified.
var target = Argument("target", "Pack");


///////////////////////////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////////////////////////


// Constructs the build state object.
Setup<BuildState>(context => {
    try {
        BuildUtilities.WriteTaskStartMessage(BuildSystem, "Setup");
        var state = new BuildState() {
            SolutionName = Argument("project", DefaultSolutionName),
            Target = target,
            Configuration = Argument("configuration", "Debug"),
            ContinuousIntegrationBuild = HasArgument("ci") || !BuildSystem.IsLocalBuild,
            Clean = HasArgument("clean"),
            Verbose = HasArgument("verbose")
        };

        if (!string.Equals(state.Target, "Clean", StringComparison.OrdinalIgnoreCase)) {
            BuildUtilities.WriteBuildStateToLog(BuildSystem, state);
        }

        return state;
    }
    finally {
        BuildUtilities.WriteTaskEndMessage(BuildSystem, "Setup");
    }
});


// Pre-task action.
TaskSetup(context => {
    BuildUtilities.WriteTaskStartMessage(BuildSystem, context.Task.Name);
});


// Post task action.
TaskTeardown(context => {
    BuildUtilities.WriteTaskEndMessage(BuildSystem, context.Task.Name);
});


///////////////////////////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////////////////////////


// Cleans up artifact and bin folders.
Task("Clean")
    .WithCriteria<BuildState>((c, state) => state.RunCleanTarget)
    .Does<BuildState>(state => {
        foreach (var pattern in new [] { $"./src/**/bin/{state.Configuration}", "./artifacts/**", "./**/TestResults/**" }) {
            BuildUtilities.WriteLogMessage(BuildSystem, $"Cleaning directories: {pattern}");
            CleanDirectories(pattern);
        }
    });


// Restores NuGet packages.
Task("Restore")
    .Does<BuildState>(state => {
        DotNetCoreRestore(state.SolutionName);
    });


// Builds the solution.
Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does<BuildState>(state => {
        var buildSettings = new DotNetCoreBuildSettings {
            Configuration = state.Configuration,
            NoRestore = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
        };

        buildSettings.MSBuildSettings.Targets.Add(state.Clean ? "Rebuild" : "Build");
        BuildUtilities.ApplyMSBuildProperties(buildSettings.MSBuildSettings, state);
        DotNetCoreBuild(state.SolutionName, buildSettings);
    });


// Builds NuGet packages.
Task("Pack")
    .IsDependentOn("Build")
    .Does<BuildState>(state => {
        var buildSettings = new DotNetCorePackSettings {
            Configuration = state.Configuration,
            NoRestore = true,
            NoBuild = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
        };

        BuildUtilities.ApplyMSBuildProperties(buildSettings.MSBuildSettings, state);
        DotNetCorePack(state.SolutionName, buildSettings);
    });


///////////////////////////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////////////////////////


RunTarget(target);