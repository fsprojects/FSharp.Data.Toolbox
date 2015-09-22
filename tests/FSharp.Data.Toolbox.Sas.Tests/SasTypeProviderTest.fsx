#r "../../bin/FSharp.Data.Toolbox.Sas.dll"

open FSharp.Data.Toolbox.SasFile
open System.IO

//let folder = Path.Combine(Directory.GetParent(__SOURCE_DIRECTORY__).Parent.FullName, @"files")

let sasFile = new FSharp.Data.Toolbox.SasProvider.SasFile<"files/acadindx.sas7bdat">()

sasFile.Header.Alignment1
sasFile.MetaData.RowCount
sasFile.FileName

// 'Data' property gives strongly-typed access to columns:
let row = sasFile.Data |> Seq.skip 5 |> Seq.head
row.acadindx
row.female

// 'Rows' gives generic access to data
let rows = sasFile.Rows
rows
|> Seq.take 100
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
    printfn "%s" line
        
    )
