namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Toolbox.Twitter")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Toolbox")>]
[<assembly: AssemblyDescriptionAttribute("F# Data-based library for various data access APIs.")>]
[<assembly: AssemblyVersionAttribute("0.17")>]
[<assembly: AssemblyFileVersionAttribute("0.17")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.17"
    let [<Literal>] InformationalVersion = "0.17"
