#if INTERACTIVE
#I "../../packages/FSharp.Data/lib/netstandard2.0"
#I "../../bin/netstandard2.0"
#r "FSharp.Data.Toolbox.Sas.dll"
#r "../../packages/NUnit/lib/NUnit.framework.dll"
#else
module FSharp.Data.Toolbox.Sas.IntegrationTests
#endif

open FSharp.Data.Toolbox.Sas

open NUnit.Framework

open System
open System.IO

let path = Path.Combine(__SOURCE_DIRECTORY__, "files")
if not <| Directory.Exists path
    || Directory.EnumerateFiles(path, "*.sas7bdat") |> Seq.isEmpty then
    let dir = __SOURCE_DIRECTORY__

    if not <| Directory.Exists path then
        Directory.CreateDirectory path |> ignore
    
    let downloadFile (line : string []) =
        let failingFiles = 
            Set ["manp.sas7bdat"; "dietnihtablea1.sas7bdat"; "Insurer_co.sas7bdat"; 
                 "hltheds2000.sas7bdat";"ymcls_p2_long_040506.sas7bdat";"pipdata.sas7bdat";
                 "Insurer_co.sas7bdat"; "Final_Candy.sas7bdat"; "f2002be2.sas7bdat";
                 "ctcodes-procedures.sas7bdat"; "candyinfo.sas7bdat"; "candy.sas7bdat";
                 "calcmilk.sas7bdat"; "yrbscol.sas7bdat"; "sorted_work_panel_data_aquifer_1.sas7bdat";
                 "osteo_analysis_data.sas7bdat"]
        async {  
            try
                let name, url = line.[1], line.[7] 
                if not (failingFiles.Contains name) && 
                   not <| url.Contains "lsu.edu" &&
                   not <| url.Contains "ats.ucla.edu" 
                then
                    printfn "Downloading test file: %s" name
                    use wc = new System.Net.WebClient()
                    return! wc.AsyncDownloadFile(Uri(url), Path.Combine(path, name)) 
            with
            | ex -> printfn "%s" (ex.Message)
            }
    // download publicly available SAS files
    Path.Combine(dir, "SAS Files.csv")
    |> File.ReadLines
    |> Seq.skip 1
    |> Seq.map (fun line -> line.Split(',') ) 
    |> Seq.map downloadFile
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore



let convert filename = 
        let csvFilename =
            Path.Combine(
                Path.GetDirectoryName filename,
                Path.GetFileNameWithoutExtension filename + ".csv")

        use sasFile = new SasFile(filename)
        use writer = File.CreateText csvFilename

        // write header
        sasFile.MetaData.Columns
        |> Array.map (fun col -> col.Name)
        |> String.concat ","
        |> writer.WriteLine

        // write lines
        sasFile.Rows
        |> Seq.iter (fun row ->
            let line =
                row
                |> Seq.map (fun value ->
                    match value with
                    | Number n -> n.ToString()
                    | Character s ->
                        let s = s.TrimEnd()
                        if s.Contains(",") then
                            "\"" + s.Replace("\"", "\"\"") + "\""
                        else s
                    | Time t -> t.ToString("HH:mm:ss")
                    | Date d -> d.ToString("yyyy-MM-dd")
                    | DateAndTime dt -> dt.ToString("O")
                    | Empty -> ""
                    )
                |> String.concat ","
            if not <| String.IsNullOrEmpty line then
                writer.WriteLine line
            )

// To find bad files
if false then
    Directory.EnumerateFiles(path, "*.sas7bdat")
    |> Seq.iter (fun path -> 
        try new SasFile(path) |> ignore
        with | Failure(msg) -> printfn "%s\n%s\n" path msg)

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
        let file = 
            Directory.EnumerateFiles(path, "*.sas7bdat")
            |> Seq.head
        new SasFile(file) |> ignore

    [<Test>]
    member x.``Reading magic number from non-SAS file is failure``() =
        let file = Path.Combine(path, "test.csv")
        if not <| File.Exists file then
            File.WriteAllText(file, "1,2,3")
        Assert.Throws<System.Exception>(fun () -> new SasFile(file) |> ignore)
        |> ignore
        //|> should throw typeof<Exception> 

    [<Test>]
    member x.``Writing CSV works``() =
        let filename = Path.Combine(path, "texas.sas7bdat")
        convert filename

    [<Test>]
    member x.``Converting all SAS7BDAT files works``() =
        Directory.EnumerateFiles(path, "*.sas7bdat")
        |> Seq.iter (fun filename -> convert filename)

    [<Test>]
    member x.``Reading all SAS7BDAT metadata works``() =
        Directory.EnumerateFiles(path, "*.sas7bdat")
        |> Seq.iter (fun filename -> 
            let sasFile = new SasFile(filename)
            dprintfn "%A" sasFile.MetaData 
            )
