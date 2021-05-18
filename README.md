# Jaahas.MSBuild.Extensions

Contains MSBuild extensions for setting copyright information and version mumbers at build time.


# Getting Started

Add the `Jaahas.MSBuild.Extensions` NuGet package to your project.


# Setting Copyright Information

Copyright information will be set via the `Copyright` MSBuild property at build time, provided that the `CopyrightStartYear` property has been set. The `Authors` property will also be used in the generated copyright message. For example:

```xml
<!-- 
Assuming that the current UTC year is 2021, sets the following copyright 
message: 

Copyright (c) 2017-2021 Joe Bloggs
-->
<PropertyGroup>
  <Authors>Joe Bloggs</Authors>
  <CopyrightStartYear>2017</CopyrightStartYear>
</PropertyGroup>
```

```xml
<!-- 
Assuming that the current UTC year is 2021, sets the following copyright 
message: 

Copyright (c) 2021 Joe Bloggs
-->
<PropertyGroup>
  <Authors>Joe Bloggs</Authors>
  <CopyrightStartYear>2021</CopyrightStartYear>
</PropertyGroup>
```


# Setting Version Numbers

The `AssemblyVersion`, `FileVersion`, `InformationalVersion`, and `Version` (i.e. package version) build parameters will be generated automatically if you specify the `VersionPropertiesInputFile` MSBuild parameter. The input file must be a JSON file that uses the following format:

```json
// Example version.json file
{
    "Major": 3,
    "Minor": 1,
    "Patch": 7,
    "PreRelease": "alpha" // Use "" if this is a release build
}
```


### Additional Build Metadata

The following build properties are used in addition to the `version.json` file:

- `BranchName`: VCS branch name, used in the informational version.
- `BuildCounter`: CI build counter, used as the revision version number.
- `BuildMetadata`: Additional build metadata, used in the informational version.
- `ContinuousIntegrationBuild`: When `false`, the build metadata will also include `unofficial` to indicate that the project was built locally instead of with a CI system.


### Example 1: Release Build via Continuous Integration System

This example shows the version numbers generated when building a release version of a project (i.e. without a pre-release version suffix) on a continuous integration system.

```json
// build\version.json

{
    "Major": 3,
    "Minor": 1,
    "Patch": 7,
    "PreRelease": ""
}
```

```xml
<!-- MyProject.csproj -->

<PropertyGroup>
  <VersionPropertiesInputFile>build\version.json</VersionPropertiesInputFile>
</PropertyGroup>
```

```
# Command Line

msbuild MyProject.csproj /p:BranchName=main /p:BuildCounter=1138 /p:ContinuousIntegrationBuild=true
```

```
# Generated Version Numbers

AssemblyVersion      = 3.1.0.0
FileVersion          = 3.1.7.1138
InformationalVersion = 3.1.7.1138+main
Version              = 3.1.7
```


### Example 2: Local Pre-Release Build

This example shows the version numbers generated when building a pre-release version of a project (i.e. with a pre-release version suffix) in a local development environment.

```json
// build\version.json

{
    "Major": 4,
    "Minor": 0,
    "Patch": 1,
    "PreRelease": "alpha"
}
```

```xml
<!-- MyProject.csproj -->

<PropertyGroup>
  <VersionPropertiesInputFile>build\version.json</VersionPropertiesInputFile>
</PropertyGroup>
```

```console
# Command Line

msbuild MyProject.csproj /p:BranchName=main /p:BuildCounter=2217
```

```console
# Generated Version Numbers

AssemblyVersion      = 4.0.0.0
FileVersion          = 4.0.1.2217
InformationalVersion = 4.0.1-alpha.2217+main#unofficial
Version              = 4.0.1-alpha.2217
```


# Building From Source

To build from source, run [build.ps1](/build.ps1) from a PowerShell prompt. Build is performed using [Cake](https://cakebuild.net). See [build.cake](/build.cake) for more information about available command-line switches.


### Release Build

To perform a release build, ensure that you specify the `--ci` flag when calling [build.ps1](/build.ps1) e.g.

```shell
.\build.ps1 --configuration Release --ci
```

The generated NuGet package can be found in the `artifacts/Packages/Release` folder.
