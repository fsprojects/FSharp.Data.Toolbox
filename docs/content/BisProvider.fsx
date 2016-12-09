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
#r "FSharp.Data.Toolbox.Bis.dll"

open FSharp.Data.Toolbox

type Prices = Bis.Dataset<"C:\\full_WEBSTATS_LONG_PP_DATAFLOW_csv.csv">

let context = Prices.ObservationFilter()
context.``Reference area`` <- 
  [ prices.``Reference area``.``CH:Switzerland`` ]
        
let result = context.Get()
