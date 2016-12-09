(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#nowarn "211"
#I "../../packages/FSharp.Data/lib/net40"
#I "../../bin"

(**
F# Data Toolbox: BIS type provider
========================

The Bank for International Settlements type provider makes BIS statistical data easily accessible 
by providing a  wrapper around the CSV files. 


Under construction
*)

open FSharp.Data.Runtime.Bis.Implementation

type prices = FSharp.Data.Toolbox.Bis.Dataset<"C:\\full_WEBSTATS_LONG_PP_DATAFLOW_csv.csv">

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    
    let context = new prices.ObservationFilter()
    context.``Reference area`` <- [ prices.``Reference area``.``CH:Switzerland`` ]
        
    let result = context.Get()
        
    0