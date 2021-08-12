// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

// This is a FAKE 5.0 script, run using
//    dotnet fake build

#r "paket: groupref fake //"

#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "netstandard"
#endif


open System
open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.DotNet.Testing
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools.Git

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let (!!) includes = (!! includes).SetBaseDirectory __SOURCE_DIRECTORY__

#if MONO
#else
//#load "packages/SourceLink.Fake/Tools/Fake.fsx"
//open SourceLink
#endif

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "FSharp.Data.Toolbox"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "F# Data-based library for various data access APIs."

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "F# Data-based library for various data access APIs."

// List of NuGet pacakges to be published...
let nugetPackages = 
  [ ("FSharp.Data.Toolbox.Twitter",
     "F# Data-based library for accessing Twitter data",
     "F# Data-based library for accessing Twitter data",
     [ "Evelina Gabasova"; "Tomas Petricek" ],
     "F# fsharp data typeprovider twitter api toolbox",
     "nuget/FSharp.Data.Toolbox.Twitter.nuspec");
     
    ("FSharp.Data.Toolbox.Sas",
     "F# Data-based library for accessing SAS data",
     "F# Data-based library for accessing SAS data",
     [ "Alexey Arestenko" ],
     "F# fsharp data sas typeprovider api toolbox",     
     "nuget/FSharp.Data.Toolbox.Sas.nuspec");
     
     ("FSharp.Data.Toolbox.Bis",
      "F# Data-based library for accessing Bank for International Settlements data",
      "F# Data-based library for accessing Bank for International Settlements data",
      [ "Darko Micic" ],
      "F# fsharp data bis typeprovider api toolbox",     
      "nuget/FSharp.Data.Toolbox.Bis.nuspec") ]

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "fsprojects" 
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "FSharp.Data.Toolbox"

// The url for the raw files hosted
let gitRaw = Environment.environVarOrDefault "gitRaw" "https://raw.github.com/fsprojects"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = ReleaseNotes.load "RELEASE_NOTES.md"
let configuration = DotNet.BuildConfiguration.fromEnvironVarOrDefault "configuration" DotNet.BuildConfiguration.Release


let genFSAssemblyInfo (projectPath) =
    let projectName = Path.GetFileNameWithoutExtension(projectPath)
    let basePath = "src/" + projectName
    let fileName = basePath + "/AssemblyInfo.fs"
    AssemblyInfoFile.createFSharp fileName
      [ AssemblyInfo.Title (projectName)
        AssemblyInfo.Product project
        AssemblyInfo.Description summary
        AssemblyInfo.Version release.AssemblyVersion
        AssemblyInfo.FileVersion release.AssemblyVersion ]

let genCSAssemblyInfo (projectPath) =
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
    let basePath = "src/" + projectName + "/Properties"
    let fileName = basePath + "/AssemblyInfo.cs"
    AssemblyInfoFile.createCSharp fileName
        [ AssemblyInfo.Title (projectName)
          AssemblyInfo.Product project
          AssemblyInfo.Description summary
          AssemblyInfo.Version release.AssemblyVersion
          AssemblyInfo.FileVersion release.AssemblyVersion ]

// Generate assembly info files with the right version & up-to-date information
Fake.Core.Target.create "AssemblyInfo" (fun _ ->
  let fsProjs =  !! "src/**/*.fsproj"
  let csProjs = !! "src/**/*.csproj"
  fsProjs |> Seq.iter genFSAssemblyInfo
  csProjs |> Seq.iter genCSAssemblyInfo
)

// --------------------------------------------------------------------------------------
// Clean build results

Fake.Core.Target.create "Clean" (fun _ ->
    Shell.cleanDirs ["bin"; "temp"]
)

Fake.Core.Target.create "CleanDocs" (fun _ ->
    Shell.cleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target.create "Build" (fun _ ->
    "FSharp.Data.Toolbox.sln"//!! "src/**/*.fsproj"//!! "FSharp.Data.Toolbox.sln"
    |> DotNet.build (fun opts -> { opts with Configuration = configuration } )

    "FSharp.Data.Toolbox.Tests.sln" //!! "tests/**/*.fsproj"//
    |> DotNet.build (fun opts -> { opts with Configuration = configuration } )
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target.create "RunTests" (fun _ ->
    !! testAssemblies
    |> Fake.DotNet.Testing.NUnit.Parallel.run (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)

#if MONO
#else
(*// --------------------------------------------------------------------------------------
// SourceLink allows Source Indexing on the PDB generated by the compiler, this allows
// the ability to step through the source code of external libraries https://github.com/ctaggart/SourceLink

Target "SourceLink" (fun _ ->
    let baseUrl = sprintf "%s/%s/{0}/%%var2%%" gitRaw (project.ToLower())
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    !! "src/**/*.fsproj"
    |> Seq.iter (fun f ->
        let proj = VsProj.LoadRelease f
        logfn "source linking %s" proj.OutputFilePdb
        let files = proj.Compiles -- "**/AssemblyInfo.fs"
        repo.VerifyChecksums files
        proj.VerifyPdbChecksums files
        proj.CreateSrcSrv baseUrl repo.Revision (repo.Paths files)
        Pdbstr.exec proj.OutputFilePdb proj.OutputFilePdbSrcSrv
    )
)
*)
#endif

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target.create "NuGet" (fun _ ->
    for project, summary, description, authors, tags, nuspec in nugetPackages do
        NuGet.NuGet (fun p -> 
            { p with   
                Authors = authors
                Project = project
                Summary = summary
                Description = description
                Version = release.NugetVersion
                ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
                Tags = tags
                OutputPath = "bin"
                AccessKey = Environment.environVarOrDefault "nugetkey" ""
                Publish = Environment.hasEnvironVar "nugetkey"
                Dependencies = [] })
            (nuspec)
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Target.create "GenerateDocs" (fun _ ->
    Shell.cleanDir ".fsdocs"
    DotNet.exec id "fsdocs" ("build --properties Configuration=Release --clean --eval --parameters fsdocs-package-version " + release.NugetVersion) |> ignore
)

Target.create "KeepRunning" (fun _ ->
   Shell.cleanDir ".fsdocs"
   DotNet.exec id "fsdocs" "watch" |> ignore
)


// --------------------------------------------------------------------------------------
// Release Scripts

Target.create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    Repository.fullclean tempDocsDir
    Shell.copyRecursive "output" tempDocsDir true |> Fake.Core.Trace.tracefn "%A"
    Staging.stageAll tempDocsDir
    Commit.exec tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

//#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
//open Octokit

Target.create "Release" ignore
Target.create "BuildPackage" ignore

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target.create "All" ignore

"Clean"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "RunTests"
  ==> "GenerateDocs"
  ==> "All"

"All" 
  ==> "NuGet"
  ==> "BuildPackage"

"CleanDocs"
  ==> "GenerateDocs"

"BuildPackage"
  ==> "Release"

Target.runOrDefault "All"
