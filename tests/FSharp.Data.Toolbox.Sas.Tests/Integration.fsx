// How to get started 
// look in the file 'SAS Files.csv' (I use Excel) and download one or more files into the 'files' directory

open System
open System.IO

//type SasList = 
//    {
//        n: int
//        name: string
//        accessed: DateTime
//        uncompressed: int
//        gzip: int
//        bzip2: int
//        xz: int
//        url: string
//        PKGversion: int
//        message: string
//        SASrelease: string
//        SAShost: string
//        OSversion: string
//        OSmaker: string
//        OSname: string
//        winunix: string
//        endianness: string
//    }

let dir = __SOURCE_DIRECTORY__

// download publicly available SAS files
let sasList =
    Path.Combine(dir, "SAS Files.csv")
    |> File.ReadLines
    |> Seq.skip 1
    |> Seq.map (fun line -> line.Split(',') )

let dir = Path.Combine(dir, "files") 
if not <| Directory.Exists dir then
    Directory.CreateDirectory dir |> ignore

sasList
|> Seq.iter (fun line -> 
    try
        let name, url = line.[1], line.[7] 
        use wc = new System.Net.WebClient()
        wc.DownloadFile (url, Path.Combine(dir, name)) 
    with
    | _ -> ignore()
    )
