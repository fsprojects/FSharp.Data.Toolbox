#if INTERACTIVE
#I "../../packages/FSharp.Data.2.0.7/lib/net40"
#I "../../bin"
#r "FSharp.Data.Toolbox.Sas.dll"
open FSharp.Data.Toolbox.SasFile
#else
module FSharp.ProjectScaffold.Tests
#endif

open FSharp.Data
open FSharp.Data.Toolbox.SasFile

open NUnit.Framework
open FsUnit

open System
open System.IO

let path = Path.Combine(__SOURCE_DIRECTORY__, "files")
if not <| Directory.Exists path
    || Directory.EnumerateFiles(path, "*.sas7bdat") |> Seq.isEmpty then
    let dir = __SOURCE_DIRECTORY__

    if not <| Directory.Exists path then
        Directory.CreateDirectory path |> ignore

    // download publicly available SAS files
    Path.Combine(dir, "SAS Files.csv")
    |> File.ReadLines
    |> Seq.skip 1
    |> Seq.map (fun line -> line.Split(',') ) 
    |> Seq.iter (fun line -> 
        try
            let name, url = line.[1], line.[7] 
            use wc = new System.Net.WebClient()
            wc.DownloadFile (url, Path.Combine(path, name)) 
        with
        | _ -> ignore()
        )

[<TestFixture>]
type ``Integration tests`` () =

    [<Test>]
    member x.``Headers are being read successfully from SAS7BDAT files``() =
        Directory.EnumerateFiles(path, "*.sas7bdat")
        |> Seq.map (fun path -> new SasFile(path))
        |> Seq.map (fun sasFile -> sasFile.Header.SasRelease )
        |> Seq.toList
        |> ignore

    [<Test>]
    member x.``Reading magic number from SAS is success``() =
        let file = Path.Combine(path, "agents.sas7bdat")
        new SasFile(file) |> ignore

    [<Test>]
    member x.``Reading magic number from non-SAS file is failure``() =
        let file = Path.Combine(path, "test.csv")
        if not <| File.Exists file then
            File.WriteAllText(file, "1,2,3")
        (fun () -> new SasFile(file) |> ignore )
        |> should throw typeof<Exception> 

    [<Test>]
    member x.``Writing CSV works``() =
        let filename = Path.Combine(path, "texas.sas7bdat")
        SasToCsv.Convert filename

    [<Test>]
    member x.``Converting all SAS7BDAT files works``() =
        Directory.EnumerateFiles(path, "*.sas7bdat")
        |> Seq.iter (fun filename -> SasToCsv.Convert filename)
        
