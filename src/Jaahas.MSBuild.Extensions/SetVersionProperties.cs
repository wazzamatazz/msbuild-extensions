using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Jaahas.MSBuild.Extensions {

    /// <summary>
    /// Custom MSBuild task that will call the <c>msbuild-version-props</c> tool to generate 
    /// compile-time version numbers for a project.
    /// </summary>
    public class SetVersionProperties : ToolTask {

        /// <summary>
        /// <c>msbuild-version-props</c> assembly name (without file extension).
        /// </summary>
        private const string MSBuildVersionPropsExe = "msbuild-version-props";

        /// <summary>
        /// Executable name for the <c>dotnet</c> command-line tool.
        /// </summary>
        private const string DotNetExe = "dotnet";

        /// <summary>
        /// The directory containing the <c>msbuild-version-props</c> tool to be invoked.
        /// </summary>
        [Required]
        public string ExeLocation { get; set; }

        /// <summary>
        /// The <c>version.json</c> input file.
        /// </summary>
        [Required]
        public string InputFile { get; set; }

        /// <summary>
        /// The path to save the generated output file to.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// The build counter to use when generating version numbers.
        /// </summary>
        public ulong BuildCounter { get; set; }

        /// <summary>
        /// The VCS branch name that is being compiled.
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Additional metadata to include in the generated informational version.
        /// </summary>
        public string BuildMetadata { get; set; }

        /// <summary>
        /// The generated assembly version.
        /// </summary>
        [Output]
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// The generated file version.
        /// </summary>
        [Output]
        public string FileVersion { get; set; }

        /// <summary>
        /// The generated informational version.
        /// </summary>
        [Output]
        public string InformationalVersion { get; set; }

        /// <summary>
        /// The generated package version.
        /// </summary>
        [Output]
        public string PackageVersion { get; set; }


        /// <inheritdoc/>
        protected override string ToolName => MSBuildVersionPropsExe;

        protected override bool ValidateParameters() {
            if (!base.ValidateParameters()) {
                return false;
            }

            try {
                VersionProperties.BuildMetadataValidator.Validate(BuildMetadata);
            }
            catch (Exception e) {
                Log.LogError(e.Message);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override string GenerateFullPathToTool() {
            if (!IsWindows()) {
                // On non-Windows platforms, we need to use the dotnet command to invoke out tool.
                return DotNetExe;
            }

            // On Windows, we can execute the EXE file directly.
            return Path.Combine(GetToolLocation(), MSBuildVersionPropsExe + ".exe");
        }


        /// <inheritdoc/>
        protected override string GenerateCommandLineCommands() {
            var builder = new CommandLineBuilder();

            if (!IsWindows()) {
                // If this is not Windows, we are using the dotnet command to invoke our tool.
                // Therefore, we need to specify the location of the tool assembly as our first
                // parameter.
                builder.AppendFileNameIfNotNull(Path.Combine(GetToolLocation(), MSBuildVersionPropsExe + ".dll"));
                // Let dotnet know that the remaining args are for the tool and not for itself.
                builder.AppendTextUnquoted(" -- ");
            }

            // The tool expects arguments in the format:
            // <input-file> [output-file] [OPTIONS]

            builder.AppendFileNameIfNotNull(InputFile);
            builder.AppendFileNameIfNotNull(OutputFile);

            if (!string.IsNullOrWhiteSpace(BranchName)) {
                builder.AppendSwitchIfNotNull("--branch ", BranchName);
            }

            if (BuildCounter > 0) {
                builder.AppendSwitchIfNotNull("--build-counter ", BuildCounter.ToString());
            }

            // If this is not a CI build, we will append "unofficial" to the build metadata.

            var buildMetadata = IsContinuousIntegrationBuild()
                ? string.IsNullOrWhiteSpace(BuildMetadata)
                    ? null
                    : BuildMetadata
                : string.IsNullOrWhiteSpace(BuildMetadata)
                    ? "unofficial"
                    : $"{BuildMetadata}.unofficial";

            if (!string.IsNullOrWhiteSpace(buildMetadata)) {
                builder.AppendSwitchIfNotNull("--build-metadata ", buildMetadata);
            }

            var result = builder.ToString();
            return result;
        }


        /// <inheritdoc/>
        public override bool Execute() {
            if (!base.Execute()) {
                return false;
            }

            var outputFile = new FileInfo(OutputFile);
            if (!outputFile.Exists) {
                return false;
            }

            var xdoc = new System.Xml.XmlDocument();
            xdoc.Load(outputFile.FullName);

            var props = xdoc["Project"]["PropertyGroup"];
            AssemblyVersion = props["AssemblyVersion"].InnerText;
            FileVersion = props["FileVersion"].InnerText;
            InformationalVersion = props["InformationalVersion"].InnerText;
            PackageVersion = props["Version"].InnerText;

            return true;
        }


        private bool IsContinuousIntegrationBuild() {
            if (!BuildEngine7.GetGlobalProperties().TryGetValue("ContinuousIntegrationBuild", out var pVal)) {
                return false;
            }

            return Convert.ToBoolean(pVal);
        }


        private string GetToolLocation() {
            if (string.IsNullOrWhiteSpace(ExeLocation)) {
                return Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(ExeLocation)) {
                Log.LogError($"{nameof(ExeLocation)} does not exist: '{ExeLocation}'");
                return Directory.GetCurrentDirectory();
            }

            return ExeLocation;
        }


        private static bool IsWindows() {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

    }
}
