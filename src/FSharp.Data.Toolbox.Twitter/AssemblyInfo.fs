﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Toolbox.Twitter")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Toolbox")>]
[<assembly: AssemblyDescriptionAttribute("F# Data-based library for various data access APIs.")>]
[<assembly: AssemblyVersionAttribute("0.7")>]
[<assembly: AssemblyFileVersionAttribute("0.7")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.7"
