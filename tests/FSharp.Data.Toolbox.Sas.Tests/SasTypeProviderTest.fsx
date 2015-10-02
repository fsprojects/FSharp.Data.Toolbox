#r "../../bin/FSharp.Data.Toolbox.Sas.dll"

open FSharp.Data.Toolbox.Sas
open System.IO 

let sasFile = new SasFileTypeProvider<"files/acadindx.sas7bdat">()

sasFile.Header.Alignment1
sasFile.MetaData.RowCount
sasFile.FileName
sasFile.Name

// 'Observations' property gives strongly-typed access to dataset variables:
let obs = sasFile.Observations |> Seq.skip 5 |> Seq.head
obs.acadindx
obs.female

// sum first 10 'reading' variable values
sasFile.Observations
|> Seq.take 10
|> Seq.sumBy ( fun obs -> obs.reading )

// calculate mean
let readingMean = 
    sasFile.Observations
    |> Seq.averageBy (fun obs -> obs.reading )

// standard deviation 
let readingStdDev =
    sqrt (( sasFile.Observations
            |> Seq.map (fun obs -> (obs.reading - readingMean) ** 2.0)
            |> Seq.sum
        ) / Seq.length sasFile.Observations )

// min
sasFile.Observations
|> Seq.map (fun obs -> obs.reading)
|> Seq.min
// ...and max
sasFile.Observations
|> Seq.map (fun obs -> obs.reading)
|> Seq.max

// multiply 'reading' by 'writing' and sum
sasFile.Observations
|> Seq.map (fun obs -> obs.reading * obs.writing)
|> Seq.sum




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


