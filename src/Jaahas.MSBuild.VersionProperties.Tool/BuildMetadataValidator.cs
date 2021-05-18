using System;
using System.Text.RegularExpressions;

namespace Jaahas.MSBuild.VersionProperties {

    /// <summary>
    /// Class for validating build metadata supplied by the caller.
    /// </summary>
    public static class BuildMetadataValidator {

        /// <summary>
        /// Regex for validating build metadata.
        /// </summary>
        private static readonly Regex s_buildMetadataRegex = new Regex(@"^[0-9A-Aa-z-]+(\.[0-9A-Aa-z-]+)*$");


        /// <summary>
        /// Validates the specified build metadata.
        /// </summary>
        /// <param name="metadata">
        ///   The build metadata.
        /// </param>
        /// <exception cref="Exception">
        ///   The <paramref name="metadata"/> is invalid.
        /// </exception>
        public static void Validate(string metadata) {
            if (string.IsNullOrEmpty(metadata)) {
                return;
            }

            if (!s_buildMetadataRegex.Match(metadata).Success) {
                throw new Exception($"Build metadata '{metadata}' is invalid. Metadata must consist of dot-delimited groups of ASCII alphanumerics and hyphens (i.e. [0-9A-Za-z-]). See https://semver.org/#spec-item-10 for details.");
            }
        }

    }
}
