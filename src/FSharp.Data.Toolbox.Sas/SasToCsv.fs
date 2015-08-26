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
        sasFile.Rows()
        |> Seq.iter (fun row ->
            let line = 
                row
                |> Seq.map (fun value -> 
                    match value with
                    | Number n -> n.ToString()
                    | Character s -> s
                    | Time t -> t.ToString("HH:mm:ss")
                    | Date d -> d.ToString()
                    | DateAndTime dt -> dt.ToString() //String.Format(format, value)
                    )
                |> String.concat "," 
            writer.WriteLine line
            )

