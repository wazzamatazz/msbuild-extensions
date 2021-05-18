This project demonstrates the use of the `Jaahas.MSBuild.VersionProperties` package to set version numbers on the compiled assembly at build time, based on a JSON file containing the version number, and some additional build properties such as a build counter.

Since the project relies on the NuGet package for `Jaahas.MSBuild.VersionProperties` being pre-built, you should build this project from the command line using `dotnet` or `msbuild`:

```
## Use dotnet to build the project.

dotnet restore --force
dotnet build
```

```
# Use msbuild to build the project

dotnet restore --force
msbuild
```
