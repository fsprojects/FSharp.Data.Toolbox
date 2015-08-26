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

[<TestFixture>]
type ``Integration tests`` () =

    [<Test>]
    member x.``Headers are being read successfully from SAS7BDAT files``() =
        Directory.EnumerateFiles(path, "*.sas7bdat")
        |> Seq.map (fun path -> new SasFile(path))
        |> Seq.map (fun sasFile -> sasFile.Header.SasRelease )
        |> Seq.toList

    member x.``Writing CSV works``() =
        let filename = Path.Combine(path, "rduschedule.sas7bdat")
        let filename = Path.Combine(path, "acadindx.sas7bdat")
        SasToCsv.Convert filename
        
//    [<Test>]
//    member x.``Reading magic number from SAS is success``() =
//        let file = Path.Combine(path, "agents.sas7bdat")
//        (SasFile file |> ignore)
        //|> should not' throw typeof<MatchFailureException> 

//    [<Test>]
//    member x.``Reading magic number from CSV is failure``() =
//        let file = Path.Combine(path, "acadindx.csv")
//        (SasFile file |> ignore)
//        |> should throw typeof<MatchFailureException>

