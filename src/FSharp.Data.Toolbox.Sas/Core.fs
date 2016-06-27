namespace FSharp.Data.Toolbox.Sas

open System

module internal ExceptionHelpers = 
    let inline invalidOp s = new InvalidOperationException(s)
    let inline divideByZero s = new DivideByZeroException(s)

[<AutoOpen>]
module Core =
    open ExceptionHelpers

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
        | NotCompressed of string // creator_proc
        | RLE of string     // creator_proc
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
        | Data
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
        | Rows              of int * int * int * int * int16 * int16 // length * row count * column count 1 * column count 2 * lcs * lcp
        | ColumnCount       of int
        | SubHeaderCounts
        | ColumnText        of byte array
        | ColumnName        of ColumnName list
        | ColumnAttributes  of ColumnAttribute list
        | ColumnFormatLabel of ColumnFormatLabel
        | ColumnList
        | DataPointer       of SubHeaderPointer * byte array
        | UnknownSubHeader  of byte array

    type Page =
        | Meta of SubHeader list
        | Data of int * byte array // block count * data
        | Mix  of SubHeader list * int * byte array
        | EmptyPage

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
        RowCount        : int
        RowSize         : int
        Columns         : Column array
        CompressionInfo : Compression
    }

    type Value =
        | Number of float
        | DateAndTime of DateTime
        | Date of DateTime
        | Time of DateTime
        | Character of string
        | Empty
//        override x.Equals value =
//            match x, value with 
//            | Empty, _ -> false
//            | Number n, (:? int  as v) -> int n = v
//            | Number n, (:? float  as v) -> n = v
//            | Number n, (:? int64  as v) -> int64 n = v
//            | Character s, (:? string as v) -> s = v
//            | DateAndTime dt, (:? DateTime as v)
//            | Date dt, (:? DateTime as v)
//            | Time dt, (:? DateTime as v) -> dt = v
//            | x, v -> sprintf "Cannot compare %A and %A" x v |> invalidOp |> raise 
        // todo: must be a way to do operations without boilerplate
        // addition
        static member inline (+) (value1, value2) =
            match value1, value2 with 
            | Empty, Empty -> Empty
            | Empty, x -> x
            | x, Empty -> x
            | Number v1, Number v2 -> Number (v1 + v2)
            | Character v1, Character v2 -> Character (v1 + v2)
            | x, y -> sprintf "Cannot add %A and %A" x y |> invalidOp |> raise 
        static member inline (+) (value, x) =
            match value with
            | Empty -> Number (float x)
            | Number n -> Number (n + float x)
            | Character s -> Character (s + string x)
            | y -> sprintf "Cannot add %A and %A" x y |> invalidOp |> raise 
        static member inline (+) (x, value) =
            match value with
            | Empty -> Number (float x)
            | Number n -> Number (n + float x)
            | Character s -> Character (string x + s)
            | y -> sprintf "Cannot add %A and %A" x y |> invalidOp |> raise 
        // substraction
        static member inline (-) (value1, value2) =
            match value1, value2 with 
            | Empty, Empty -> Empty
            | Empty, Number x -> Number -x
            | x, Empty -> x
            | Number v1, Number v2 -> Number (v1 - v2)
            | x, y -> sprintf "Cannot substract %A from %A" x y |> invalidOp |> raise 
        static member inline (-) (value, x) =
            match value with
            | Empty -> Number (float -x)
            | Number n -> Number (n - float x)
            | y -> sprintf "Cannot substract %A from %A" y x |> invalidOp |> raise
        static member inline (-) (x, value) =
            match value with
            | Empty -> Number (float x)
            | Number n -> Number (float x - n)
            | y -> sprintf "Cannot substract %A from %A" x y |> invalidOp |> raise 
        // multiplication
        static member inline (*) (value1, value2) =
            match value1, value2 with 
            | Empty, Empty -> Empty
            | Empty, x -> Empty
            | x, Empty -> Empty
            | Number v1, Number v2 -> Number (v1 * v2)
            | Character v1, Number v2 -> Character (String.replicate (int v2) v1 )
            | Number v2, Character v1 -> Character (String.replicate (int v2) v1 )
            | x, y -> sprintf "Cannot multiply %A by %A" x y |> invalidOp |> raise 
        static member inline (*) (value, x) =
            match value with
            | Empty -> Empty
            | Number n -> Number (n * float x)
            | Character s -> Character (String.replicate (int x) s)
            | y -> sprintf "Cannot multiply %A by %A" x y |> invalidOp |> raise 
        static member inline (*) (x, value) =
            match value with
            | Empty -> Empty
            | Number n -> Number (n * float x)
            | Character s -> Character (String.replicate (int x) s)
            | y -> sprintf "Cannot multiply %A by %A" x y |> invalidOp |> raise 
        // division
        static member inline (/) (value1, value2) =
            match value1, value2 with 
            | Empty, x -> Empty
            | Empty, Empty -> "Cannot divide by Empty value" |> divideByZero |> raise 
            | x, Empty -> "Cannot divide by Empty value" |> divideByZero |> raise 
            | Number v1, Number v2 when v2 <> 0. -> Number (v1 / v2)
            | Number v1, Number v2 -> "Cannot divide by zero Number" |> divideByZero |> raise 
            | x, y -> sprintf "Cannot divide %A by %A" x y |> invalidOp |> raise 
        static member inline (/) (value, x) =
            match value with
            | Empty -> Empty
            | Number n when float x <> 0. -> Number (n / float x)
            | Number n -> "Cannot divide by zero" |> divideByZero |> raise 
            | y -> sprintf "Cannot divide %A by %A" x y |> invalidOp |> raise 
        static member inline (/) (x, value) =
            match value with
            | Empty -> "Cannot divide by Empty value" |> divideByZero |> raise 
            | Number n when n <> 0. -> Number (n / float x)
            | Number n -> "Cannot divide by zero Number" |> divideByZero |> raise 
            | y -> sprintf "Cannot divide %A by %A" x y |> invalidOp |> raise 

        static member Zero = Number 0.0
        static member inline DivideByInt (value, n) =
            match value with
            | Empty -> Empty
            | Number x when n <> 0 -> Number (x / float n)
            | Number x -> "Cannot divide by zero" |> divideByZero |> raise 
            | x -> sprintf "Cannot divide %A by %A" x n |> invalidOp |> raise 
        static member inline Pow (value, n) =
            match value with
            | Empty -> Empty
            | Number x -> Number (x ** float n)
            | x -> sprintf "Cannot raise %A to power %A" x n |> invalidOp |> raise 
        static member inline Sqrt value =
            match value with
            | Empty -> Empty
            | Number x -> Number (sqrt x)
            | x -> sprintf "Cannot take square root of %A" x |> invalidOp |> raise 


    let inline decompress meta offset len (data: _ array) =
        match meta.CompressionInfo with
        | NotCompressed _ -> data.[offset .. meta.RowSize + offset - 1]
        // unpack run-length encoded data
        | Compression.RLE _ ->
            let off = offset
            let mutable resultIndex = 0
            let mutable srcIndex = 0
            let result = Array.zeroCreate meta.RowSize

            for i = 0 to len - 1 do
                if i = srcIndex then
                    let marker = data.[off + srcIndex]  &&& 0xF0uy
                    let firstByteEnd = data.[off + srcIndex]  &&& 0x0Fuy |> int
                    let copyOffset =
                        if marker < 0x80uy then data.[off + srcIndex + 1] &&& 0xFFuy |> int
                        else 0 // not used for big markers, gives out of range error
                    try
                    match marker with
                    | 0x00uy ->
                        if srcIndex <> len - 1 then
                            let bytesToCopy = copyOffset + 64 + firstByteEnd*256
                            Array.blit
                                data (off + srcIndex + 2)
                                result resultIndex
                                bytesToCopy
                            srcIndex    <- srcIndex + bytesToCopy + 1
                            resultIndex <- resultIndex + bytesToCopy
                    | 0x40uy ->
                        let bytesToCopy = copyOffset + firstByteEnd*16
                        for z = 0 to bytesToCopy + 17 do
                            result.[resultIndex] <- data.[off + srcIndex + 2]
                            resultIndex <- resultIndex + 1
                        srcIndex <- srcIndex + 2
                    | 0x60uy ->
                        for z = 0 to firstByteEnd*256 +
                            copyOffset + 16 do
                            result.[resultIndex] <- 0x20uy
                            resultIndex <- resultIndex + 1
                        srcIndex <- srcIndex + 1
                    | 0x70uy ->
                        for z = 0 to firstByteEnd*256 + 
                            copyOffset + 16 do
                            result.[resultIndex] <- 0uy
                            resultIndex <- resultIndex + 1
                        srcIndex <- srcIndex + 1
                    | 0x80uy ->
                        let bytesToCopy = min (firstByteEnd + 1) (len - srcIndex - 1)
                        Array.blit
                            data (off + srcIndex + 1)
                            result resultIndex
                            bytesToCopy
                        srcIndex <- srcIndex + bytesToCopy
                        resultIndex <- resultIndex + bytesToCopy
                    | 0x90uy ->
                        let bytesToCopy = min (firstByteEnd + 17) (len - srcIndex - 1)
                        Array.blit
                            data (off + srcIndex + 1)
                            result resultIndex
                            bytesToCopy
                        srcIndex <- srcIndex + bytesToCopy
                        resultIndex <- resultIndex + bytesToCopy
                    | 0xA0uy ->
                        let bytesToCopy = min (firstByteEnd + 33) (len - srcIndex - 1)
                        Array.blit
                            data (off + srcIndex + 1)
                            result resultIndex
                            bytesToCopy
                        srcIndex <- srcIndex + bytesToCopy
                        resultIndex <- resultIndex + bytesToCopy
                    | 0xB0uy ->
                        let bytesToCopy = min (firstByteEnd + 49) (len - srcIndex - 1)
                        Array.blit
                            data (off + srcIndex + 1)
                            result resultIndex
                            bytesToCopy
                        srcIndex <- srcIndex + bytesToCopy
                        resultIndex <- resultIndex + bytesToCopy
                    | 0xC0uy ->
                        for z = 0 to firstByteEnd + 2 do
                            result.[resultIndex] <- data.[off + srcIndex + 1]
                            resultIndex <- resultIndex + 1
                        srcIndex <- srcIndex + 1
                    | 0xD0uy ->
                        for z = 0 to firstByteEnd + 1 do
                            result.[resultIndex] <- 0x40uy
                            resultIndex <- resultIndex + 1
                    | 0xE0uy ->
                        for z = 0 to firstByteEnd + 1 do
                            result.[resultIndex] <- 0x20uy
                            resultIndex <- resultIndex + 1
                    | 0xF0uy ->
                        for z = 0 to firstByteEnd + 1 do
                            result.[resultIndex] <- 0uy
                            resultIndex <- resultIndex + 1
                    | _ -> failwithf "Cannot decompress. Unknown marker: %i" marker

                    srcIndex <- srcIndex + 1
                    with
                    | _ -> failwithf "Exception at srcIndex: %i, resultIndex: %i" srcIndex resultIndex
            result
        // remote differential compression
        | Compression.RDC -> data

    [<AutoOpen>]
    module Helpers =
        open System.Diagnostics

        // byte array conversions
        let ToInt bytes =
            BitConverter.ToInt32(bytes, 0)

        let ToShort bytes =
            BitConverter.ToInt16(bytes, 0)

        let ToLong bytes =
            BitConverter.ToInt64(bytes, 0)

        let ToDouble bytes =
            let maybeNumber =
                if Array.length bytes % 8 <> 0 then
                    let len = (Array.length bytes + 7) / 8 * 8
                    let bytes' = Array.zeroCreate len
                    Array.blit bytes 0 bytes' (len - bytes.Length) bytes.Length
                    BitConverter.ToDouble(bytes', 0)
                else
                    BitConverter.ToDouble(bytes, 0)
            if Double.IsNaN maybeNumber then
                None
            else
                Some maybeNumber

        let ToByte (bytes: byte array) =
            bytes.[0]

        let ToStr bytes =
            let chars =
                bytes
                |> Array.filter (fun b -> b <> 0uy)
                |> Array.map char
            new string(chars)

        // prepend zero bytes if there are fewer than 8 bytes
        let expand (bytes:byte[]) =
            match bytes.Length with
            | 8 -> bytes
            | _ -> let bytes8 = Array.create 8 0uy
                   Array.blit bytes 0 bytes8 (8 - bytes.Length) bytes.Length
                   bytes8

        let ToDateTime (bytes:byte[]) =
            let seconds = BitConverter.ToDouble(expand bytes, 0)
            if Double.IsNaN seconds then
                None
            else
                Some <| DateTime(1960, 1, 1).AddSeconds seconds


        let ToDate (bytes:byte[]) =
            let days = BitConverter.ToDouble(expand bytes, 0)
            if Double.IsNaN days then
                None
            else
                Some <| DateTime(1960, 1, 1).AddDays days

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

        let endian bytes = function
        | Big    -> Array.rev bytes
        | Little -> bytes

        /// Python-like array slicing
        let inline slice (arr: _ array) (start, len) =
            arr.[start .. start + len - 1]

        let inline sliceEndian (arr: _ array) (start, len) =
            endian arr.[start .. start + len - 1]

        let inline dprintfn fmt =
            Printf.ksprintf Debug.WriteLine fmt
