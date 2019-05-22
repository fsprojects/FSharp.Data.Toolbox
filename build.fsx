open Fake
open Fake
// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.DotNet

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.Tools.Git
open System
open System.IO
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
let gitRaw = Fake.Core.Environment.environVarOrDefault "gitRaw" "https://raw.github.com/fsprojects"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = 
    File.read "RELEASE_NOTES.md"
    |> Fake.Core.ReleaseNotes.parse

let genFSAssemblyInfo (projectPath) =
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
    let basePath = "src/" + projectName
    let fileName = basePath + "/AssemblyInfo.fs"
    Fake.DotNet.AssemblyInfoFile.createFSharp fileName
      [ Fake.DotNet.AssemblyInfo.Title (projectName)
        Fake.DotNet.AssemblyInfo.Product project
        Fake.DotNet.AssemblyInfo.Description summary
        Fake.DotNet.AssemblyInfo.Version release.AssemblyVersion
        Fake.DotNet.AssemblyInfo.FileVersion release.AssemblyVersion ]

let genCSAssemblyInfo (projectPath) =
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
    let basePath = "src/" + projectName + "/Properties"
    let fileName = basePath + "/AssemblyInfo.cs"
    Fake.DotNet.AssemblyInfoFile.createCSharp fileName
        [ Fake.DotNet.AssemblyInfo.Title (projectName)
          Fake.DotNet.AssemblyInfo.Product project
          Fake.DotNet.AssemblyInfo.Description summary
          Fake.DotNet.AssemblyInfo.Version release.AssemblyVersion
          Fake.DotNet.AssemblyInfo.FileVersion release.AssemblyVersion ]

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

Fake.Core.Target.create "Build" (fun _ ->
    !! "FSharp.Data.Toolbox.sln"
    |> Fake.DotNet.MSBuild.runRelease id "" "Rebuild"
    |> ignore

    !! "FSharp.Data.Toolbox.Tests.sln"
    |> Fake.DotNet.MSBuild.runRelease id "" "Rebuild"
    |> ignore
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Fake.Core.Target.create "RunTests" (fun _ ->
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

Fake.Core.Target.create "NuGet" (fun _ ->
    for project, summary, description, authors, tags, nuspec in nugetPackages do
        Fake.DotNet.NuGet.NuGet.NuGet (fun p -> 
            { p with   
                Authors = authors
                Project = project
                Summary = summary
                Description = description
                Version = release.NugetVersion
                ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
                Tags = tags
                OutputPath = "bin"
                AccessKey = Fake.Core.Environment.environVarOrDefault "nugetkey" ""
                Publish = Fake.Core.Environment.hasEnvironVar "nugetkey"
                Dependencies = [] })
            (nuspec)
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Fake.Core.Target.create "GenerateReferenceDocs" (fun _ ->
    let (exitCode, messages) =
        Fake.DotNet.Fsi.exec 
            (fun p ->
                { p with 
                    WorkingDirectory = "docs/tools"
                     
                    } )
            // Script to run
            "generate.fsx"
            // script arguments
            ["--define:RELEASE"; "--define:REFERENCE"] 
    match exitCode with
    | 0 -> 
        messages
        |> List.iter Fake.Core.Trace.trace
    | _ -> 
        messages
        |> List.iter Fake.Core.Trace.traceError
        failwith "generating reference documentation failed"            
)
let generateHelp' fail debug =
    let args =
        if debug then ["--define:HELP"]
        else ["--define:RELEASE"; "--define:HELP"]
    let (exitCode, messages) =
        Fake.DotNet.Fsi.exec 
            id
            // Script to run
            "generate.fsx"
            // script arguments
            args
    match exitCode with
    | 0 -> 
        Fake.Core.Trace.traceImportant "Help generated"
    | _ -> 
        messages
        |> List.iter Fake.Core.Trace.traceError
        Fake.Core.Trace.traceImportant "generating reference documentation failed"

let generateHelp fail =
    generateHelp' fail false

Fake.Core.Target.create "GenerateHelp" (fun _ ->
    File.delete "docs/content/release-notes.md"
    Shell.copyFile "docs/content/" "RELEASE_NOTES.md"
    Shell.rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    File.delete "docs/content/license.md"
    Shell.copyFile "docs/content/" "LICENSE.txt"
    Shell.rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp true
)

Fake.Core.Target.create "GenerateHelpDebug" (fun _ ->
    File.delete "docs/content/release-notes.md"
    Shell.copyFile "docs/content/" "RELEASE_NOTES.md"
    Shell.rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    File.delete "docs/content/license.md"
    Shell.copyFile "docs/content/" "LICENSE.txt"
    Shell.rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp' true true
)

Fake.Core.Target.create "KeepRunning" (fun _ ->    
    use watcher = new FileSystemWatcher(DirectoryInfo("docs/content").FullName,"*.*")
    watcher.EnableRaisingEvents <- true
    watcher.Changed.Add(fun e -> generateHelp false)
    watcher.Created.Add(fun e -> generateHelp false)
    watcher.Renamed.Add(fun e -> generateHelp false)
    watcher.Deleted.Add(fun e -> generateHelp false)

    Fake.Core.Trace.traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.EnableRaisingEvents <- false
    watcher.Dispose()
)

Fake.Core.Target.create "GenerateDocs" ignore

let createIndexFsx lang =
    let content = """(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../../bin"

(**
F# Project Scaffold ({0})
=========================
*)
"""
    let targetDir = "docs/content" @@ lang
    let targetFile = targetDir @@ "index.fsx"
    Directory.ensure targetDir
    System.IO.File.WriteAllText(targetFile, System.String.Format(content, lang))

Fake.Core.Target.create "AddLangDocs" (fun _ ->
    let args = System.Environment.GetCommandLineArgs()
    if args.Length < 4 then
        failwith "Language not specified."

    args.[3..]
    |> Seq.iter (fun lang ->
        if lang.Length <> 2 && lang.Length <> 3 then
            failwithf "Language must be 2 or 3 characters (ex. 'de', 'fr', 'ja', 'gsw', etc.): %s" lang

        let templateFileName = "template.cshtml"
        let templateDir = "docs/tools/templates"
        let langTemplateDir = templateDir @@ lang
        let langTemplateFileName = langTemplateDir @@ templateFileName

        if System.IO.File.Exists(langTemplateFileName) then
            failwithf "Documents for specified language '%s' have already been added." lang

        Directory.ensure langTemplateDir
        Shell.copy langTemplateDir [ templateDir @@ templateFileName ]

        createIndexFsx lang)
)

// --------------------------------------------------------------------------------------
// Release Scripts

Fake.Core.Target.create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    Repository.fullclean tempDocsDir
    Shell.copyRecursive "docs/output" tempDocsDir true |> Fake.Core.Trace.tracefn "%A"
    Staging.stageAll tempDocsDir
    Fake.Tools.Git.Commit.exec tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

Fake.Core.Target.create "Release" (fun _ ->
    Staging.stageAll ""
    Fake.Tools.Git.Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion
    
    // release on github
    // createClient (getBuildParamOrDefault "github-user" "") (getBuildParamOrDefault "github-pw" "")
    // |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes 
    // // TODO: |> uploadFile "PATH_TO_FILE"    
    // |> releaseDraft
    // |> Async.RunSynchronously
)

Fake.Core.Target.create "BuildPackage" ignore

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Fake.Core.Target.create "All" ignore

"Clean"
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "RunTests"
  =?> ("GenerateReferenceDocs",Fake.Core.BuildServer.isLocalBuild && not Fake.Core.Environment.isMono)
  =?> ("GenerateDocs",Fake.Core.BuildServer.isLocalBuild && not Fake.Core.Environment.isMono)
  ==> "All"
  =?> ("ReleaseDocs",Fake.Core.BuildServer.isLocalBuild && not Fake.Core.Environment.isMono)

"All" 
#if MONO
#else
//  =?> ("SourceLink", Pdbstr.tryFind().IsSome )
#endif
  ==> "NuGet"
  ==> "BuildPackage"

"CleanDocs"
  ==> "GenerateHelp"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"

"CleanDocs"
  ==> "GenerateHelpDebug"

"GenerateHelp"
  ==> "KeepRunning"
    
"ReleaseDocs"
  ==> "Release"

"BuildPackage"
  ==> "Release"

Fake.Core.Target.runOrDefault "All"
