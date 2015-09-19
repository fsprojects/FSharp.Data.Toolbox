// Gert-Jan van der Kamp 2015-05-20
// 
// This code is based on the work of Matt Shotwell 
// http://cran.r-project.org/web/packages/sas7bdat/vignettes/sas7bdat.pdf


namespace SAS

module MetaData =

    open System
    open System.IO

    type Endianness = 
        | Little = 0
        | Big = 1


    type OSType = 
        | UNIX = 1
        | Windows = 2

   
    let byte2str (raw:byte[]) start length = 
        let arr = 
            raw.[start..start+length-1] 
            |> Array.filter (fun b -> b <> 0uy) // get rid off empty bytes
            |> Array.map char
        let s = new System.String(arr)
        s.Trim()

    let sasDate raw start = 
        let secs = BitConverter.ToDouble(raw, start )
        DateTime(1960, 1, 1) + TimeSpan.FromSeconds(secs)

    
    let sasString (raw:byte[])  = 
        let arr = 
            raw 
            |> Array.filter (fun b -> b <> 0uy) // get rid off empty bytes
            |> Array.map char
        let s = new System.String(arr)
        s.Trim()
           
    
    type Header = {
        A1          : int
        A2          : int
        DataSet     : string
        HeaderSize  : int
        PageCount   : int
        PageSize    : int
        FilePath    : string
        DateCreated : DateTime
        DateModified: DateTime
        OSType      : OSType
        Endianness  : Endianness
    }


    let readHeader filePath (raw:byte[]) = 
        let a1 = if raw.[32] = 33uy then 4 else 0 
        let a2 = if raw.[35] = 33uy then 4 else 0  
        {
            A1 = a1 
            A2 = a2
            DataSet     = byte2str raw 92 64
            HeaderSize  = BitConverter.ToInt32(raw, 196+a2)
            PageSize    = BitConverter.ToInt32(raw, 200+a2)
            PageCount   = BitConverter.ToInt32(raw, 204+a2) 
            FilePath    = filePath
            DateCreated = sasDate raw (164+a1)
            DateModified = sasDate raw (172+a1)
            Endianness  = match raw.[37] with
                            | 00uy -> Endianness.Big 
                            | 01uy -> Endianness.Little
                            | _ -> failwith "unknown Endianness"                                
            OSType      = match char raw.[39] with
                            | '1' -> OSType.UNIX
                            | '2' -> OSType.Windows
                            | _ ->
                                let b = char raw.[39]
                                let msg = sprintf "unknown OSType: %c in file %s" b filePath    
                                failwith msg
       }  
         

    type ColumnName = {
        textIndex  : int
        offset : int
        length : int
    }


    type ColumnAttribute = {
        offset : int
        width : int
        typ : byte
    }

    
    type SubHeader =
        | RowSize of int * int // size * count
        | ColumnSize of int // count
        | SubHeaderCounts
        | ColumnText of byte array
        | ColumnNames of ColumnName array
        | ColumnAttributes of ColumnAttribute array
        | ColumnFormatLabel
        | ColumnList
        | Unknown of byte[]

    
    let readSubHeader (raw:byte array) =
        if (Array.length raw) < 3 
            then Unknown raw
            else 
            match BitConverter.ToString raw.[0..3] with
                | "F7-F7-F7-F7" -> RowSize (BitConverter.ToInt32(raw.[20..23],0),BitConverter.ToInt32(raw.[24..35],0))
                | "F6-F6-F6-F6" -> ColumnSize (BitConverter.ToInt32(raw.[4..7],0) )
                | "00-FC-FF-FF" -> SubHeaderCounts
                | "FD-FF-FF-FF" -> ColumnText raw
                | "FF-FF-FF-FF" -> 
                    let rem = int (BitConverter.ToInt64(raw.[4..11],0))
                    let count = rem / 8
                    let names = 
                        Array.init count (fun i-> raw.[12+i*8..12+i*8+7])
                        |> Array.map (fun ba -> 
                        {
                            textIndex = int (BitConverter.ToInt16(ba.[0..1],0))
                            offset  = int (BitConverter.ToInt16(ba.[2..3],0)) + 4
                            length =  int (BitConverter.ToInt16(ba.[4..5],0))
                        })               
                    ColumnNames (names |> Array.filter (fun n -> n.length > 0))
                | "FC-FF-FF-FF" -> 
                    let rem = BitConverter.ToInt32(raw.[4..7],0)
                    let count = (rem - 8)/ 12
                    let attr = 
                         Array.init count (fun i-> raw.[12+i*12..12+i*12+11])
                        |> Array.map (fun ba -> 
                        {
                            offset = BitConverter.ToInt32(ba.[0..3],0) 
                            width  = BitConverter.ToInt32(ba.[4..7],0)
                            typ =  ba.[10]
                        })               
                    ColumnAttributes attr
                | "FE-FB-FF-FF" -> ColumnFormatLabel 
                | "FE-FF-FF-FF" -> ColumnList
                | x -> Unknown raw


    let readPageSubHeaders (page:byte[]) =
        let subHeaderCount = int (BitConverter.ToInt16(page.[20..21],0))   
        [0..subHeaderCount-1] 
        |> List.map (fun hi -> page.[24+hi*12 .. 24+hi*12+11]) // read subheaderpointer bytes
        |> List.map (fun shp -> (BitConverter.ToInt32(shp.[0..3],0), BitConverter.ToInt32(shp.[4..7],0))) // get start and lengths of subheaders
        |> List.map (fun (s, l) -> page.[s .. s+l-1]) // read subheaders bytes
        |> List.map readSubHeader // and cast to correct header type


    type Page =
        | Meta of SubHeader list
        | Data of int * byte[] // block_count * payload
        | Mix of SubHeader list * int * byte[] // shs * block_count * payload
        | AMD 
        

    let readPage (page:byte[]) =        
        let pageType = BitConverter.ToInt16(page.[16..17],0)               
        let blockCount = int (BitConverter.ToInt16(page.[18..19],0))         
        match (pageType) with
            | 0s -> Meta (readPageSubHeaders page )
            | 256s -> Data (blockCount, page)
            | 512s -> Mix ((readPageSubHeaders page), blockCount, page)
            | 1024s -> AMD 
            | _ -> failwith "unknown pageType"


    let readChunk size (file:FileStream) = 
        let buf = Array.zeroCreate size            
        file.Read(buf, 0, size) |> ignore
        buf    


    type ColumnType = 
        | Numeric 
        | Text 


    type Column = {
        Ordinal : int
        Name : string
        Offset : int
        Width : int
        Type : ColumnType
    }


    let fileHeaderAndPages fileName = 
        let file = File.OpenRead fileName
        let header = file |> readChunk 288 |> (readHeader fileName) // read first 288 btyes of header
        file |> readChunk (header.HeaderSize - 288) |> ignore       // skip rest of header (header itself says how big it is)
        let pages = seq {                                           // create seq of pages in rest of file
            for i in [1..header.PageCount] do
                yield file |> readChunk header.PageSize |> readPage
            file.Close()
        }
        (header, pages)   // return both


    let metapages pages =   // helper function to only get pages that contain metadata (skip rest, big speedup for large files)
            pages
            |> Seq.takeWhile (fun p -> 
                match p with 
                | Meta shs -> true
                | Mix (shs, i, r) -> true
                | _ -> false
            )

             
    let getColumnsAndRowsize pages =  // colect subheaders from all pages into one list       
        
        let subHeaderSeq = seq {    // collect subheaders from all pages with metadata
            for mp in metapages pages do
                match mp with 
                | Meta shs -> yield! shs
                | Mix  (shs, i, r) -> yield! shs
                | _ -> failwith "There should only be Meta or Mix here"                
        } 
        let subHeaders = subHeaderSeq |> Seq.toList
                   
        let (rowSize, rowCount) = match subHeaders.[0] with RowSize (s, c)-> (s, c) // only one of these                
        let textHeaders  =      // collect all headers of type ColumnText
            subHeaders
            |> List.choose (fun h ->
                match h with 
                | ColumnText raw -> Some (new String(Array.map char raw))  // extract the string within
                | _-> None
            )        
        
        let names = 
            subHeaders
            |> List.choose (
                fun h -> 
                match h with 
                | ColumnNames n  -> Some n
                | _ -> None)
            |> Array.concat 

        let attr  = 
            subHeaders
            |> List.choose (
                fun h -> 
                match h with 
                | ColumnAttributes a  -> Some a 
                | _ -> None)
            |> Array.concat    

        let columns = 
            (names, attr) // reconstruct column info from the pieces that once must have been so close together o_O
            ||> Array.mapi2 (fun i n a -> 
            {
                Ordinal = i
                Name= textHeaders.[n.textIndex].[n.offset..n.offset+n.length-1]
                Offset = a.offset
                Width=a.width
                Type= match a.typ with 
                        | 1uy -> Numeric
                        | 2uy -> Text
                        | _ -> failwith "Unknown ColumnType"
            })
        (columns, rowSize) // return columns, rowsize


    let  generateTypeCode (fileName:string) =      
    
        let (header, p) = fileHeaderAndPages fileName
        let pages = p |> metapages |> Seq.toList

        let (columns, rowSize) = getColumnsAndRowsize pages

        let getType (c:Column) =
            match c.Type with 
            | Numeric -> "float"
            | Text    -> "string"

        let getConverter (c:Column) =
            match c.Type with 
            | Numeric -> 
                match c.Width with 
                |3 -> sprintf "float (BitConverter.ToSingle(Array.append [| 0uy |] raw.[%i..%i], 0))"   c.Offset (c.Offset + 2)
                |4 -> sprintf "float (BitConverter.ToSingle(raw, %i))"                                  c.Offset
                |5 -> sprintf "BitConverter.ToDouble(Array.append [| 0uy; 0uy; 0uy |] raw.[%i..%i], 0)" c.Offset (c.Offset + 4)
                |6 -> sprintf "BitConverter.ToDouble(Array.append [| 0uy; 0uy |] raw.[%i..%i], 0)"      c.Offset (c.Offset + 5)
                |7 -> sprintf "BitConverter.ToDouble(Array.append [| 0uy; |] raw.[%i..%i], 0)"          c.Offset (c.Offset + 6)
                |8 -> sprintf "BitConverter.ToDouble(raw, %i)"                                          c.Offset
                |_ -> 
                    let msg = sprintf "unknown numeric width %i in file %s!" c.Width fileName
                    failwith msg
            | Text -> sprintf "byte2str raw %i %i" c.Offset c.Width
            

        let columnCode = 
            columns 
            |> Array.map (fun c -> sprintf "        ``%s`` : %s\n" c.Name (getType c))
            |> String.Concat    

        let readCode = 
            columns 
            |> Array.map (fun c -> sprintf "                ``%s`` = %s\n" c.Name (getConverter c))
            |> String.Concat

        let template = @"
    type #typeName = {
#columnCode
    }
    with 
        static member rowSize = #rowSize
        static member fromRaw (raw:byte[]) = {
#fromRaw    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*#typeName.rowSize .. dataStart+(i+1)*#typeName.rowSize-1]) // chop page 
                |> Array.map #typeName.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*#typeName.rowSize .. dataStart+(i+1)*#typeName.rowSize-1]) // chop page 
                |> Array.map #typeName.fromRaw
            | _ -> Array.empty<#typeName>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @#filePath
            for pp in p do
                yield! #typeName.p2a pp
        }    
" 
        let code = template.Replace("#typeName", header.DataSet).Replace("#columnCode", columnCode).Replace("#rowSize", rowSize.ToString()).Replace("#fromRaw",readCode).Replace("#filePath","\"" + header.FilePath + "\"")
        code


    let generateLibCode path =
        let top = @"
namespace SasLib 

    open SAS.MetaData
    open System.IO
    open System
        "
        let sw = new StringWriter()
        sw.Write top
        Directory.GetFiles path
        |> Seq.filter (fun f -> f.ToLower().EndsWith ".sas7bdat")  
        |> Seq.map generateTypeCode 
        |> Seq.iter (fun c -> sw.Write c)
        sw.ToString()

