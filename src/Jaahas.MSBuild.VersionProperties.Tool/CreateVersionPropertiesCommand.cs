using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Jaahas.MSBuild.VersionProperties {

    /// <summary>
    /// Command for creating MSBuild version properties.
    /// </summary>
    public class CreateVersionPropertiesCommand : AsyncCommand<CreateVersionPropertiesCommand.Settings> {

        /// <summary>
        /// Settings for <see cref="CreateVersionPropertiesCommand"/>/
        /// </summary>
        public class Settings : CommandSettings {

            /// <summary>
            /// The input file.
            /// </summary>
            [CommandArgument(0, "<input-file>")]
            [Description("A JSON file containing the major, minor, patch, and pre-release version numbers.")]
            public string InputFile { get; set; } = default!;

            /// <summary>
            /// The output file to write the MSBuild properties to.
            /// </summary>
            [CommandArgument(1, "[output-file]")]
            [Description("The output file to write the MSBuild properties to. If not specified, the properties will be written to stdout.")]
            public string? OutputFile { get; set; }

            /// <summary>
            /// VCS branch name for the build.
            /// </summary>
            [CommandOption("--branch")]
            [Description("The VCS branch name for the build.")]
            public string? Branch { get; set; }

            /// <summary>
            /// Build counter as supplied by the CI system.
            /// </summary>
            [CommandOption("--build-counter")]
            [Description("The CI build counter.")]
            public ulong BuildCounter { get; set; }

            /// <summary>
            /// Optional build metadata to include in the informational version number.
            /// </summary>
            [CommandOption("--build-metadata")]
            [Description("Additional build metadata to include in the informational version number.")]
            public string? BuildMetadata { get; set; }

        }


        /// <inheritdoc/>
        public override ValidationResult Validate(CommandContext context, Settings settings) {
            var inputFile = GetFileInfo(settings.InputFile);

            if (!inputFile.Exists) {
                return ValidationResult.Error($"File does not exist: '{inputFile.FullName}'");
            }

            BuildMetadataValidator.Validate(settings.BuildMetadata!);

            return ValidationResult.Success();
        }


        /// <inheritdoc/>
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var inputFile = GetFileInfo(settings.InputFile);

            VersionInfo? versionInfo;

            using (var reader = inputFile.OpenRead()) {
                versionInfo = await JsonSerializer.DeserializeAsync<VersionInfo>(reader).ConfigureAwait(false);
            }

            if (versionInfo == null) {
                versionInfo = new VersionInfo() {
                    Major = 0,
                    Minor = 0,
                    Patch = 1
                };
            }

            var assemblyVersion = GetAssemblyVersion(versionInfo);
            var assemblyFileVersion = GetFileVersion(versionInfo, settings.BuildCounter);
            var informationalVersion = GetInformationalVersion(versionInfo, settings.BuildCounter, settings.Branch, settings.BuildMetadata);
            var packageVersion = GetPackageVersion(versionInfo, settings.BuildCounter);

            string props;

            using (var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(typeof(Program), "Version.props.template")!)) {
                var xml = await reader.ReadToEndAsync().ConfigureAwait(false);

                props = xml
                    .Replace("#ASSEMBLY_VERSION", assemblyVersion)
                    .Replace("#FILE_VERSION", assemblyFileVersion)
                    .Replace("#INFORMATIONAL_VERSION", informationalVersion)
                    .Replace("#PACKAGE_VERSION", packageVersion);


            }

            if (string.IsNullOrWhiteSpace(settings.OutputFile)) {
                Console.WriteLine(props);
            }
            else {
                var outputFile = GetFileInfo(settings.OutputFile!);
                outputFile.Directory.Create();
                using (var writer = new StreamWriter(outputFile.Open(FileMode.Create, FileAccess.Write))) {
                    writer.Write(props);
                }
            }

            return 0;
        }


        /// <summary>
        /// Gets the fully-qualified path for the specified file.
        /// </summary>
        /// <param name="path">
        ///   The relative or absolute path to the file.
        /// </param>
        /// <returns>
        ///   A <see cref="FileInfo"/> object representing the file.
        /// </returns>
        private static FileInfo GetFileInfo(string path) {
            if (!Path.IsPathRooted(path)) {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }

            return new FileInfo(path);
        }


        /// <summary>
        /// Generates the assembly version number.
        /// </summary>
        /// <param name="versionInfo">
        ///   The supplied version information.
        /// </param>
        /// <returns>
        ///   The assembly version number.
        /// </returns>
        private static string GetAssemblyVersion(VersionInfo versionInfo) {
            return $"{versionInfo.Major}.{versionInfo.Minor}.0.0";
        }


        /// <summary>
        /// Generates the file version number.
        /// </summary>
        /// <param name="versionInfo">
        ///   The supplied version information.
        /// </param>
        /// <param name="buildCounter">
        ///   The build counter supplied by the CI system.
        /// </param>
        /// <returns>
        ///   The file version number.
        /// </returns>
        private static string GetFileVersion(VersionInfo versionInfo, ulong buildCounter) {
            return $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}.{buildCounter}";
        }


        /// <summary>
        /// Generates the package version number.
        /// </summary>
        /// <param name="versionInfo">
        ///   The supplied version information.
        /// </param>
        /// <param name="buildCounter">
        ///   The build counter supplied by the CI system. This is only used in pre-release 
        ///   package version numbers.
        /// </param>
        /// <returns>
        ///   The package version number.
        /// </returns>
        private static string GetPackageVersion(VersionInfo versionInfo, ulong buildCounter) {
            return string.IsNullOrWhiteSpace(versionInfo.PreRelease)
                ? $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}"
                : $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}-{versionInfo.PreRelease}.{buildCounter}";
        }


        /// <summary>
        /// Generates the informational version number.
        /// </summary>
        /// <param name="versionInfo">
        ///   The supplied version information.
        /// </param>
        /// <param name="buildCounter">
        ///   The build counter supplied by the CI system.
        /// </param>
        /// <param name="branch">
        ///   The VCS branch name for the build.
        /// </param>
        /// <param name="buildMetadata">
        ///   The build metadata supplied by the caller.
        /// </param>
        /// <returns>
        ///   The informational version number.
        /// </returns>
        private static string GetInformationalVersion(VersionInfo versionInfo, ulong buildCounter, string? branch, string? buildMetadata) {
            var sb = new StringBuilder();

            sb.Append($"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}");
            if (!string.IsNullOrWhiteSpace(versionInfo.PreRelease)) {
                sb.Append($"-{versionInfo.PreRelease}");
            }

            sb.Append($".{buildCounter}");

            if (!string.IsNullOrWhiteSpace(branch)) {
                sb.Append($"+{branch}");
            }

            if (!string.IsNullOrWhiteSpace(buildMetadata)) {
                sb.Append($"#{buildMetadata}");
            }

            return sb.ToString();
        }

    }
}
