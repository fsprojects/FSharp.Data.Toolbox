namespace FSharp.Data.Toolbox.SasFile

open System

[<AutoOpen>]
module Core =

    type Bits = 
        | X86
        | X64

    type Endianness = 
        | Little
        | Big

    type Platform = 
        | Windows
        | Unix
        | UnknownPlatform

    type Encoding = 
        | ASCII
        | UTF8

    type Compression = 
        | NotCompressed
        | RLE
        | RDC

    type Header = {
        Alignment1            : int
        Alignment2            : int

        Bits                  : Bits
        PageBitOffset         : int
        WordLength            : int
        SubHeaderPointerLength: int

        Endianness            : Endianness
        Platform              : Platform
        DataSet               : string
        FileType              : string
        DateCreated           : DateTime
        DateModified          : DateTime
        HeaderSize            : int
        PageSize              : int
        PageCount             : int
        SasRelease            : string
        ServerType            : string
        OsVersion             : string
        OsName                : string
        Compression           : Compression
    } 

    type SubHeaderPointer = {
         Offset      : int
         Length      : int
         Compression : byte
         Type        : byte
    }

    type SubHeaderType = 
        | Truncated         
        | Rows              
        | ColumnCount       
        | SubHeaderCount
        | ColumnText        
        | ColumnName        
        | ColumnAttributes  
        | ColumnFormatLabel 
        | ColumnList        
        | Unknown           

    type ColumnName = {
        ColumnNameOffset : int16
        ColumnNameLength : int16
        TextIndex        : int16
    }

    type ColumnAttribute = {
        ColumnAttrOffset : int
        ColumnAttrWidth  : int
        ColumnType       : byte
    }

    type ColumnFormatLabel = {
        TextSubHeaderFormat: int16
        TextSubHeaderLabel : int16
        ColumnFormatOffset : int16
        ColumnFormatLength : int16
        ColumnLabelOffset  : int16
        ColumnLabelLength  : int16
    }

    type SubHeader = 
        | Truncated
        | Rows              of int * int // length * count
        | ColumnCount       of int
        | SubHeaderCounts 
        | ColumnText        of byte array
        | ColumnName        of ColumnName list
        | ColumnAttributes  of ColumnAttribute list
        | ColumnFormatLabel of ColumnFormatLabel
        | ColumnList 
        | UnknownSubHeader  of byte array

    type Page = 
        | Meta of SubHeader list
        | Data of int * byte array // block count * data
        | Mix  of SubHeader list * int * byte array
        | AMD  of SubHeader list * int * byte array

    type ColumnType = 
        | Numeric
        | Text

    type Column = {
        Ordinal  : int
        Name     : string
        Type     : ColumnType
        Length   : int
        Format   : string
        //Informat : string
        Label    : string
        Offset   : int
    }

    // metadata of SAS file
    type MetaData = {
        RowCount : int
        RowSize  : int
        Columns  : Column list
    }

    type Value = 
        | Number of double
        | DateAndTime of DateTime
        | Date of DateTime
        | Time of DateTime
        | Character of string
        
    type Row = Value list

    //type EncodingErrorsAction = 
    //    | Ignore
    //
    //type AlignErrorAction = 
    //    | Ignore


    // byte array conversions
    let ToInt bytes = 
        BitConverter.ToInt32(bytes, 0)

    let ToShort bytes = 
        BitConverter.ToInt16(bytes, 0)

    let ToLong bytes = 
        BitConverter.ToInt64(bytes, 0)

    let ToDouble bytes = 
        BitConverter.ToDouble(bytes, 0)

    let ToByte (bytes: byte array) = 
        bytes.[0]

    let ToStr bytes = 
        let chars = 
            bytes
            |> Array.filter (fun b -> b <> 0uy)
            |> Array.map char
        new string(chars)

    let ToDateTime bytes = 
        let seconds = BitConverter.ToDouble(bytes, 0)
        DateTime(1960, 1, 1).AddSeconds seconds

    let ToDate bytes = 
        let days = BitConverter.ToDouble(bytes, 0)
        DateTime(1960, 1, 1).AddDays days

    /// Split a string to two-char substrings and convert to byte array
    let FromHex str = 
        str
        |> Seq.map Char.ToUpper 
        |> Seq.filter (fun c -> c >= '0' && c <= '9' || c >= 'A' && c <= 'F') // only hex digits
        |> Seq.windowed 2 // split by two
        |> Seq.mapi (fun index value -> index, value) // index
        |> Seq.filter (fun (index, value) -> index % 2 = 0)
        |> Seq.map (fun (_, value) -> new string(value)) // two-char strings
        |> Seq.map (fun hex -> Convert.ToByte(hex, 16)) // to bytes
        |> Array.ofSeq 


    /// Check if element is in a set
    let InSet elem set' = 
        Set.intersect (set [ elem ]) set' 
        |> Set.isEmpty 

    /// Python-like array slicing
    let inline slice (arr: ^a array) (start, len) = 
        arr.[start .. start + len - 1]
