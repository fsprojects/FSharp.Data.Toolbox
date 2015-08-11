#if INTERACTIVE
#I "../../packages/FSharp.Data.2.0.7/lib/net40"
#I "../../bin"
#r @"FSharp.Data.dll"
#r "FSharp.Data.Toolbox.Sas.dll"
open FSharp.Data.Toolbox.Sas
#else
module FSharp.ProjectScaffold.Tests
#endif

open FSharp.Data
open FSharp.Data.Toolbox.Sas.SasFile

open NUnit.Framework
open FsUnit

open System
open System.IO

let path = Path.Combine(Directory.GetParent(__SOURCE_DIRECTORY__).Parent.FullName, @"files\acadindx.sas7bdat")

[<TestFixture>]
type ``Integration tests`` () =

    [<Test>]
    member x.``Headers are being read successfully from SAS7BDAT files``() =
        Directory.GetFiles(path, "*.sas7bdat")
        |> Array.map (fun path -> new SasFile(path))
        |> ignore 

    [<Test>]
    member x.``Reading magic number from SAS is success``() =
        let file = Path.Combine(path, "agents.sas7bdat")
        (SasFile file |> ignore)
        //|> should not' throw typeof<MatchFailureException> 

    [<Test>]
    member x.``Reading magic number from CSV is failure``() =
        let file = Path.Combine(path, "acadindx.csv")
        (SasFile file |> ignore)
        |> should throw typeof<MatchFailureException>
