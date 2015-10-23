open System
open System.IO
open System.Text

open SasFile

[<EntryPoint>]
let main argv =     
    use sas = new SasFile @"C:\home\ar\projects\sas\Test\c\acadindx.sas7bdat"
    let lines = sas.ReadLines()


    
    let _ = System.Console.ReadKey()
    0
