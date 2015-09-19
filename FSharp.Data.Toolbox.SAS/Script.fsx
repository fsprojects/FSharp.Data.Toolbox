// How to get started 
// look in the file SAS Files.csv (I use Excel) and download one or more files into the directory specified on the next line
let folder = @"D:\SAS Files 3"

#load "SAS.MetaData.fs"
open SAS.MetaData
open System.IO

// helper function to display data
open System.Windows.Forms
let grid data =
    let frm = new Form()
    let grd = new DataGridView()    
    grd.Dock <- DockStyle.Fill
    frm.Controls.Add grd
    grd.DataSource <- (data |> Seq.toArray)
    frm.Show()
    frm.Focus() |> ignore

// shows info on sasfiles in folder 
Directory.GetFiles folder
|> Array.filter (fun path -> path.ToLower().EndsWith(".sas7bdat"))
|> Array.map (fun path -> 
    let (header, pages) = fileHeaderAndPages path
    header) 
|> grid


// generate f# file in this directlry 
// CHANGE LINE BELOW TO PROJECT FOLDER (is there any way to make #load of a variable? )
let path = @"D:\Documents\GitHub\FSharp.Data.Toolbox\FSharp.Data.Toolbox.SAS\Generated.fs"
let code = generateLibCode folder
File.WriteAllLines(path, [|code|])

// load library we just created
#load @"D:\Documents\GitHub\FSharp.Data.Toolbox\FSharp.Data.Toolbox.SAS\Generated.fs"

// display one of the types we just created (change to select file you downloaded)
SasLib.WHEAT.Data |> grid

