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
        Columns         : Column list
        CompressionInfo : Compression
    }

    type Value =
        | Number of float
        | DateAndTime of DateTime
        | Date of DateTime
        | Time of DateTime
        | Character of string
        | Empty

    type Row = Value list

    //type EncodingErrorsAction =
    //    | Ignore
    //
    //type AlignErrorAction =
    //    | Ignore

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
                        for z = 0 to copyOffset + 16 do
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

        let ToDateTime bytes =
            let seconds = BitConverter.ToDouble(bytes, 0)
            if Double.IsNaN seconds then
                None
            else
                Some <| DateTime(1960, 1, 1).AddSeconds seconds

        let ToDate bytes =
            let days = BitConverter.ToDouble(bytes, 0)
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