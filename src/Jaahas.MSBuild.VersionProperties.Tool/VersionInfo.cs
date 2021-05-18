namespace Jaahas.MSBuild.VersionProperties {

    /// <summary>
    /// DTO that describes the different components of a project's version number.
    /// </summary>
    public class VersionInfo {

        /// <summary>
        /// Major version.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Minor version.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Patch version.
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// Pre-release suffix.
        /// </summary>
        public string? PreRelease { get; set; }

    }
}
