namespace FSharp.Data.Toolbox.SasFile

open System
open System.IO

module SasToCsv =
    let Convert filename =
        let csvFilename =
            Path.Combine(
                Path.GetDirectoryName filename,
                Path.GetFileNameWithoutExtension filename + ".csv")

        use sasFile = new SasFile(filename)
        use writer = File.CreateText csvFilename

        // write header
        sasFile.MetaData.Columns
        |> List.map (fun col -> col.Name)
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

