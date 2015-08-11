namespace FSharp.Data.Toolbox.Sas

[<AutoOpen>]
module Core =

    open System

    type Bits = 
        | X86
        | X64

    type Endianness = 
        | Little
        | Big

    type Platform = 
        | Windows
        | Unix
        | Unknown

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
         Offset : int
         Length : int
         Compression : byte
         Type : byte
     }

    type SubHeader = 
        | Truncated
        | RowSize
        | ColumnSize
        | SubheaderCounts
        | ColumnText
        | ColumnName
        | ColumnAttributes
        | FormatAndLabel
        | ColumnList
        | Data

     type ColumnName = {
        ColNameOffset             : int
        ColNameLength             : int
        TextIndex          : int
    }

    type ColumnAttribute = {
        ColAttrOffset        : int
        ColAttrWidth         : int
        ColumnType    : byte
    }

    //type SubHeader =
    //    | RowSize          of int * int // size * count
    //    | ColumnSize       of int // count
    //    | SubHeaderCounts
    //    | ColumnText       of byte array
    //    | ColumnNames      of ColumnName array
    //    | ColumnAttributes of ColumnAttribute array
    //    | ColumnFormatLabel
    //    | ColumnList
    //    | UnknownSubHeader of byte array

    //type Page = 
    //    | Meta of SubHeader list
    //    | Data of int * byte array // block count * data
    //    | Mix  of SubHeader list * int * byte array
    //    | AMD  of SubHeader list * int * byte array


    //type EncodingErrorsAction = 
    //    | Ignore
    //
    //type AlignErrorAction = 
    //    | Ignore

    type Values =
        | AByte            of byte
        | Int              of int
        | Long             of int64
        | Float            of float
        | Date             of DateTime
        | Str              of string * int

    let ToInt bytes = 
        BitConverter.ToInt32(bytes, 0)

    let ToShort bytes = 
        BitConverter.ToInt16(bytes, 0)

    let ToLong bytes = 
        BitConverter.ToInt64(bytes, 0)

    let ToStr bytes = 
        let chars = 
            bytes
            |> Array.filter (fun b -> b <> 0uy)
            |> Array.map char
        let s = new string(chars)
        s.Trim()

    let ToDate bytes = 
        let seconds = BitConverter.ToDouble(bytes, 0)
        DateTime(1960, 1, 1).AddSeconds seconds

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

