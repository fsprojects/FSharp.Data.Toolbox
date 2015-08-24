#r "../../bin/FSharp.Data.Toolbox.Sas.dll"

open FSharp.Data.Toolbox.SasFile
open System.IO

//let folder = Path.Combine(Directory.GetParent(__SOURCE_DIRECTORY__).Parent.FullName, @"files")

let sasFile = new FSharp.Data.Toolbox.SasProvider.SasFile<"files/acadindx.sas7bdat">()


sasFile.Header.Alignment1
sasFile.MetaData.RowCount
sasFile.FileName

