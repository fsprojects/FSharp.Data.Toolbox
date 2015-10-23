namespace FSharp.Data.Toolbox.Sas

open System
open System.Diagnostics
open System.IO

type Library (directory) =
    do if not <| Directory.Exists directory then
            failwithf "Directory '%s' not found." directory

    let files = 
        Directory.EnumerateFiles (directory, "*.sas7bdat")
        |> Seq.map (fun filename -> new SasFile(filename) )

    member x.Files = files
