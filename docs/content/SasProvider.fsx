(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#nowarn "211"
#I "../../packages/FSharp.Data/lib/net40"
#I "../../bin"

(**
F# Data Toolbox: SAS dataset type provider
========================

The SAS dataset (sas7bdat) type provider allows exploratory 
programming with SAS files and provides native access 
to SAS datasets. No SAS software or OLE DB providers required.

Opening a SAS dataset file
-----------------------------------------
*)
// First, reference the locations where F# Data and 
// F# Data Toolbox are located (using '#I' is required here!)
#I @"packages/FSharp.Data.Toolbox.Sas.0.3/lib/net40"
#I @"packages/FSharp.Data.2.1.1/lib/net40"

// The SAS reference needs to come before FSharp.Data.dll
// (see the big warning box below for more!)
#r "FSharp.Data.Toolbox.Sas.dll"
#r "FSharp.Data.dll"
open FSharp.Data.Toolbox.Sas

(**
<div class="well well-small" style="margin:0px 70px 0px 20px;">

**WARNING**: Unfortunately, F# Interactive is quite sensitive to how you
reference the packages when using F# Data Toolbox. To make the type provider work 
correctly in F# Interactive, you need to:

 - Use the `#I` directive to reference the path where the two libraries are located
   (rather than usign `#r` with a full relative path)

 - Reference `FSharp.Data.Toolbox.Sas.dll` *before* referencing `FSharp.Data.dll` 
   as on the first two lines above. 

</p></div>

### Open SAS dataset by passing file name to SasFile type
*)
[<Literal>] 
let sasPath = @"../../tests/FSharp.Data.Toolbox.Sas.Tests/files/acadindx.sas7bdat"

let sasFile = new SasFileTypeProvider<sasPath>()

(**
After openning the dataset, you can call methods to access 
SAS metadata and the data itself.

Accessing metadata
---------------------------------------
The following examples show how to access meta-information about SAS dataset.

*)
let datasetName = sasFile.Header.DataSet.Trim()
let architecture = sasFile.Header.Bits
let rowCount = sasFile.MetaData.RowCount

// Get a list of dataset columns
let cols = sasFile.MetaData.Columns
printfn "Number of columns: %d" (cols |> Seq.length)
printfn "Names of columns: %s"  (cols |> Seq.map (fun col -> col.Name) |> String.concat ", ")


(**
Accessing data in a strongly-typed fashion
-----------------------------------
Good for exploratory programming. IntelliSense access to column names. 
*)
// read sixth row of data
let row = sasFile.Observations |> Seq.skip 5 |> Seq.head
printfn "Column 'id' value: %A" row.id
printfn "Column 'reading' value: %A" row.reading
  
// sum first 10 'reading' variable values
sasFile.Observations
|> Seq.take 10
|> Seq.sumBy ( fun obs -> obs.reading )

// calculate mean
let readingMean = 
    sasFile.Observations
    |> Seq.averageBy (fun obs -> obs.reading )

// standard deviation 
let readingStdDev =
    sqrt (( sasFile.Observations
            |> Seq.map (fun obs -> (obs.reading - readingMean) ** 2.0)
            |> Seq.sum
        ) / Seq.length sasFile.Observations )

// min
sasFile.Observations
|> Seq.map (fun obs -> obs.reading)
|> Seq.min
// ...and max
sasFile.Observations
|> Seq.map (fun obs -> obs.reading)
|> Seq.max

// multiply 'reading' by 'writing' and sum
sasFile.Observations
|> Seq.map (fun obs -> obs.reading * obs.writing)
|> Seq.sum

// ..equivalent to:
query {
    for obs in sasFile.Observations do
    sumBy (obs.reading * obs.writing)
}

(**
Accessing data in a generic way
--------------------------------------
Can be used for bulk data processing or converting SAS files to text files.
*)
let valueToText value = 
    match value with
    | Number n -> n.ToString()
    | Character s -> s.Trim()
    | Time t -> t.ToString("HH:mm:ss")
    | Date d -> d.ToString("yyyy-MM-dd")
    | DateAndTime dt -> dt.ToString("O")
    | Empty -> ""

sasFile.Rows
    |> Seq.take 100
    |> Seq.iter (fun row ->
        let line =
            row
            |> Seq.map valueToText
            |> String.concat "," 
        printfn "%s" line )

(**
We can display the data in a grid. 
*)
open System.Windows.Forms

// Create a window with a grid
let frm = 
    new Form(TopMost = true, Visible = true, 
        Text = "F# Type Provider for SAS: " + System.IO.Path.GetFileName sasFile.FileName, 
        Width = 600, Height = 600)
let grid = new DataGridView(Dock = DockStyle.Fill, ReadOnly = true)
let btn = new Button(Text = "Read next page", Dock = DockStyle.Bottom)
let status = new StatusBar(ShowPanels = true, Dock = DockStyle.Bottom)
let pageStatus = new StatusBarPanel(Text = "Page")
let recordStatus = new StatusBarPanel(Text = "Records", Width = 300 )
status.Panels.Add pageStatus
status.Panels.Add recordStatus
frm.Controls.Add grid
frm.Controls.Add btn
frm.Controls.Add status

let pageSize = 100

let read page = 
    sasFile.Observations 
    |> Seq.skip (pageSize*(page - 1))
    |> Seq.truncate pageSize

// Add columns
let columns = sasFile.MetaData.Columns
grid.ColumnCount <- columns.Length
for i = 0 to columns.Length - 1 do
    grid.Columns.[i].HeaderText <- columns.[i].Name

// Display data
let show page =
    let data = read page
    grid.Rows.Clear()
    pageStatus.Text <- sprintf "Page %i" page
    recordStatus.Text <- sprintf "Records %i-%i of %i" 
        <| (page-1)*pageSize + 1 
        <| min (page*pageSize) sasFile.MetaData.RowCount 
        <| sasFile.MetaData.RowCount
    for row in data do
        let values = [| for value in row -> valueToText value |]

        let gridRow = new DataGridViewRow()
        gridRow.CreateCells grid

        for col = 0 to columns.Length - 1 do
            gridRow.Cells.[col].Value <- values.[col]

        grid.Rows.Add gridRow |> ignore
    
let mutable page = 1
btn.Click.Add(fun _ -> 
    if page*pageSize < sasFile.MetaData.RowCount then
        page <- page + 1
    else 
        page <- 1
    show page 
    )

show page
(**
![SAS dataset viewer](img/SasViewer.png)
*)
