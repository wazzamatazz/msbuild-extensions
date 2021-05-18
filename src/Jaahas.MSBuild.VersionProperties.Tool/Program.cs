using System.Threading.Tasks;

using Spectre.Console.Cli;

namespace Jaahas.MSBuild.VersionProperties {

    /// <summary>
    /// Tool for generating MSBuild version numbers for a project.
    /// </summary>
    class Program {

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">
        ///   The command-line arguments.
        /// </param>
        /// <returns>
        ///   The command status code.
        /// </returns>
        public static async Task<int> Main(string[] args) {
            var app = new CommandApp<CreateVersionPropertiesCommand>();
            return await app.RunAsync(args).ConfigureAwait(false);
        }

    }
}
