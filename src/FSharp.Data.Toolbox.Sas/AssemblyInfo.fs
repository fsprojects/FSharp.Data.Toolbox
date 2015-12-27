namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Toolbox.Sas")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Toolbox")>]
[<assembly: AssemblyDescriptionAttribute("F# Data-based library for various data access APIs.")>]
[<assembly: AssemblyVersionAttribute("0.12")>]
[<assembly: AssemblyFileVersionAttribute("0.12")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.12"
