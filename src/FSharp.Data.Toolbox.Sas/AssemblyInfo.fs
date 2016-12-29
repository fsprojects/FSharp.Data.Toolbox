namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Toolbox.Sas")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Toolbox")>]
[<assembly: AssemblyDescriptionAttribute("F# Data-based library for various data access APIs.")>]
[<assembly: AssemblyVersionAttribute("0.19")>]
[<assembly: AssemblyFileVersionAttribute("0.19")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.19"
    let [<Literal>] InformationalVersion = "0.19"
