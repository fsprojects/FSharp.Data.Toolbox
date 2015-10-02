namespace FSharp.Data.Toolbox.Sas

open System
open System.Diagnostics
open System.IO

type SasFile (filename) =

    let name = Path.GetFileNameWithoutExtension filename

    let reader =
        if not <| File.Exists filename then
            failwith "File '%s' not found" filename
        dprintfn "Reading SAS file '%s'" filename
        new BinaryReader(File.Open
            (filename, FileMode.Open, FileAccess.Read, FileShare.Read))

    let header =
        let headerBytes =
            reader.BaseStream.Seek(0L, SeekOrigin.Begin) |> ignore
            let header = reader.ReadBytes META_LENGTH
            if header.Length < META_LENGTH then
                failwith "File is not SAS7BDAT (too short)"
            // Check magic number
            if header.[..Magic.Length - 1] <> Magic then
                dprintfn "Magic number mismatch"
            header

        let readHeader offset len =
            slice headerBytes (offset, len)

        let bits, alignment2 =
            let align1bytes = readHeader ALIGN_1_OFFSET ALIGN_1_LENGTH
            if align1bytes = U64_BYTE_CHECKER_VALUE then
                X64, ALIGN_2_VALUE
            else X86, 0
        let alignment1 =
            let align2bytes = readHeader ALIGN_2_OFFSET ALIGN_2_LENGTH
            if align2bytes = ALIGN_1_CHECKER_VALUE then
                ALIGN_1_VALUE
            else 0

        let pageBitOffset, wordLength, subheaderPointerLength =
            match bits with
            | X86 -> PAGE_BIT_OFFSET_X86, WORD_LENGTH_X86, SUBHEADER_POINTER_LENGTH_X86
            | X64 -> PAGE_BIT_OFFSET_X64, WORD_LENGTH_X64, SUBHEADER_POINTER_LENGTH_X64

        let totalAlignment = alignment1 + alignment2

        let endianness =
            let endianByte = readHeader ENDIANNESS_OFFSET ENDIANNESS_LENGTH
            match endianByte with
            | [| 00uy |] -> Big
            | [| 01uy |] -> Little
            | _    -> failwith "Unknown endianness"

        let readHeader' offset len =
            sliceEndian headerBytes (offset, len) endianness

        let platform =
            let platformBytes = readHeader PLATFORM_OFFSET PLATFORM_LENGTH
            match platformBytes with
            | "1"B -> Unix
            | "2"B -> Windows
            | _    -> UnknownPlatform

        let dataSet =
            let datasetBytes = readHeader DATASET_OFFSET DATASET_LENGTH
            ToStr datasetBytes

        let fileType =
            let fileTypeBytes = readHeader FILE_TYPE_OFFSET FILE_TYPE_LENGTH
            ToStr fileTypeBytes

        // Timestamp is epoch 01/01/1960
        let dateCreated =
            let dateCreatedBytes = readHeader' (DATE_CREATED_OFFSET + alignment1) DATE_CREATED_LENGTH
            match ToDateTime dateCreatedBytes with
            | Some date -> date
            | None -> new DateTime(1960, 1, 1)

        let dateModified =
            let dateModifiedBytes = readHeader' (DATE_MODIFIED_OFFSET + alignment1) DATE_MODIFIED_LENGTH
            match ToDateTime dateModifiedBytes with
            | Some date -> date
            | None -> new DateTime(1960, 1, 1)

        let headerSize =
            let headerSizeBytes = readHeader' (HEADER_SIZE_OFFSET + alignment1) HEADER_SIZE_LENGTH
            let headerSize = ToInt headerSizeBytes
            if bits = X64 && headerSize <> 8*1024 then
                dprintfn "Header size (%d) doesn't match word length (X64)" headerSize
            headerSize

        let pageSize =
            let pageSizeBytes = readHeader' (PAGE_SIZE_OFFSET + alignment1) PAGE_SIZE_LENGTH
            ToInt pageSizeBytes

        let pageCount =
            let pageCountBytes = readHeader' <| PAGE_COUNT_OFFSET + alignment1 <| PAGE_COUNT_LENGTH + alignment2
            ToInt pageCountBytes

        let sasRelease =
            let sasReleaseBytes = readHeader (SAS_RELEASE_OFFSET + totalAlignment) SAS_RELEASE_LENGTH
            ToStr sasReleaseBytes

        let serverType =
            let serverTypeBytes = readHeader (SAS_SERVER_TYPE_OFFSET + totalAlignment) SAS_SERVER_TYPE_LENGTH
            ToStr serverTypeBytes

        let osVersion =
            let osVersionBytes = readHeader (OS_VERSION_NUMBER_OFFSET + totalAlignment) OS_VERSION_NUMBER_LENGTH
            ToStr osVersionBytes

        let osName =
            let osNameOffset, osNameLen =
                if OS_NAME_OFFSET + totalAlignment <> 0 then
                    OS_NAME_OFFSET  + totalAlignment, OS_NAME_LENGTH
                else
                    OS_MAKER_OFFSET + totalAlignment, OS_MAKER_LENGTH
            let osNameBytes =
                readHeader
                <| min osNameOffset (headerBytes.Length - osNameLen)
                <| osNameLen
            ToStr osNameBytes

        {   Alignment1             =  alignment1
            Alignment2             =  alignment2
            Bits                   =  bits
            PageBitOffset          =  pageBitOffset
            WordLength             =  wordLength
            SubHeaderPointerLength =  subheaderPointerLength
            Endianness             =  endianness
            Platform               =  platform
            DataSet                =  dataSet
            FileType               =  fileType
            DateCreated            =  dateCreated
            DateModified           =  dateModified
            HeaderSize             =  headerSize
            PageSize               =  pageSize
            PageCount              =  pageCount
            SasRelease             =  sasRelease
            ServerType             =  serverType
            OsVersion              =  osVersion
            OsName                 =  osName }


    let readPage (page: byte array) =
        let slice' bytes (offset, len) =
            sliceEndian bytes (offset, len) header.Endianness

        let pageBytes offset len =
            let offset = offset + header.PageBitOffset
            slice' page (offset, len)

        let pageType =
            let bytes = pageBytes PAGE_TYPE_OFFSET PAGE_TYPE_LENGTH
            ToShort bytes

        let blockCount =
            let bytes = pageBytes BLOCK_COUNT_OFFSET BLOCK_COUNT_LENGTH
            ToShort bytes |> int

        /// Process page metadata
        let readSubHeaders () =

            let subHeaderCount =
                let bytes = pageBytes SUBHEADER_COUNT_OFFSET SUBHEADER_COUNT_LENGTH
                ToShort bytes |> int

            let readSubHeaderPointerBytes n =
                let subHeaderPointerOffset = SUBHEADER_POINTERS_OFFSET + header.PageBitOffset
                let totalOffset = subHeaderPointerOffset + n*header.SubHeaderPointerLength
                slice page (totalOffset, 2*header.WordLength + 2)

            // process_subheader_pointers
            let readSubHeaderPointer (subBytes: byte array) =
                let word = header.WordLength
                { Offset      = slice' subBytes (0,    word)    |> ToInt
                  Length      = slice' subBytes (word, word)    |> ToInt
                  Compression = slice  subBytes (2*word,     1) |> ToByte
                  Type        = slice  subBytes (2*word + 1, 1) |> ToByte }

            let readSubHeader subHeaderPointer =
                let subHeaderType =
                    if subHeaderPointer.Compression = TRUNCATED_SUBHEADER_ID ||
                       subHeaderPointer.Length = 0 then SubHeaderType.Truncated
                    else
                        let subSignature = pageBytes (subHeaderPointer.Offset - header.PageBitOffset) header.WordLength
                        let subHeaderMatched, subHeader = SignatureToHeaderColumn.TryGetValue subSignature
                        if not subHeaderMatched &&
                            (subHeaderPointer.Compression = COMPRESSED_SUBHEADER_ID ||
                             subHeaderPointer.Compression = 0uy) &&
                             subHeaderPointer.Type = COMPRESSED_SUBHEADER_TYPE then
                             SubHeaderType.Data
                        elif subHeaderMatched then subHeader
                        else SubHeaderType.Unknown

                let offset = subHeaderPointer.Offset
                let word = header.WordLength

                match subHeaderType with
                | SubHeaderType.Truncated-> Truncated
                | SubHeaderType.Rows ->
                    let rowLength =
                        slice' page (offset + ROW_LENGTH_OFFSET_MULTIPLIER*word, word)
                        |> ToInt
                    let rowCount =
                        slice' page (offset + ROW_COUNT_OFFSET_MULTIPLIER*word, word)
                        |> ToInt

                    let lcsOffset = offset + match header.Bits with X64 -> 682 | X86 -> 354
                    let lcpOffset = offset + match header.Bits with X64 -> 706 | X86 -> 378
//                    let rowCountMix =
//                        slice page (offset + ROW_COUNT_ON_MIX_PAGE_OFFSET_MULTIPLIER*word, word)
//                        |> ToInt
                    let lcs = slice' page (lcsOffset, 2) |> ToShort
                    let lcp = slice' page (lcpOffset, 2) |> ToShort
                    if lcs > 0s then
                        let creator = slice page (offset + (match header.Bits with X86 -> 16 | X64 -> 20), int lcs)
                                        |> ToStr
                        ()
                    let colCountP1 =
                        slice' page (offset + COL_COUNT_P1_MULTIPLIER*word, word)
                        |> ToInt
                    let colCountP2 =
                        slice' page (offset + COL_COUNT_P2_MULTIPLIER*word, word)
                        |> ToInt

                    Rows (rowLength, rowCount, colCountP1, colCountP2, lcs, lcp)
                | SubHeaderType.ColumnCount ->
                    let offset = offset + word
                    let colCount = slice' page (offset, word) |> ToInt
                    ColumnCount colCount
                | SubHeaderType.SubHeaderCount ->
                    SubHeaderCounts // Not sure what to do here yet
                | SubHeaderType.ColumnText ->
                    let textBlockSize = slice' page (offset + word, TEXT_BLOCK_SIZE_LENGTH) |> ToShort
                    let columnNames = slice page (offset, int textBlockSize + word)
                    ColumnText columnNames
                | SubHeaderType.ColumnName ->
                    let offset = offset + word
                    let count = (subHeaderPointer.Length - 2*word - 12) / 8
                    let names =
                        [1 .. count]
                        |> List.map (fun n ->
                            let offset = offset + COLUMN_NAME_POINTER_LENGTH*n
                            {
                            TextIndex        = slice' page (offset +
                                                COLUMN_NAME_TEXT_SUBHEADER_OFFSET,
                                                COLUMN_NAME_TEXT_SUBHEADER_LENGTH) |> ToShort
                            ColumnNameOffset = slice' page (offset +
                                                COLUMN_NAME_OFFSET_OFFSET,
                                                COLUMN_NAME_OFFSET_LENGTH) |> ToShort
                            ColumnNameLength = slice' page (offset +
                                                COLUMN_NAME_LENGTH_OFFSET,
                                                COLUMN_NAME_LENGTH_LENGTH) |> ToShort
                            })
                        |> List.filter (fun col -> col.ColumnNameLength > 0s )
                    ColumnName names
                | SubHeaderType.ColumnAttributes ->
                    let offset = offset + word
                    let count = (subHeaderPointer.Length - 2*word - 12) / (word + 8)

                    let attributes =
                        [0 .. count - 1]
                        |> List.map (fun n ->
                            let offset = offset + (word + 8)*n
                            {
                            ColumnAttrOffset = slice' page (offset + COLUMN_DATA_OFFSET_OFFSET,
                                                word) |> ToInt
                            ColumnAttrWidth  = slice' page (offset + word + COLUMN_DATA_LENGTH_OFFSET,
                                                COLUMN_DATA_LENGTH_LENGTH) |> ToInt
                            ColumnType       = slice' page (offset + word + COLUMN_TYPE_OFFSET,
                                                COLUMN_TYPE_LENGTH) |> ToByte
                            })
                    ColumnAttributes attributes
                | SubHeaderType.ColumnFormatLabel ->
                    let offset = subHeaderPointer.Offset + 3*word
                    let colFormatLabel =
                        {
                        TextSubHeaderFormat = slice' page (offset +
                                                COLUMN_FORMAT_TEXT_SUBHEADER_INDEX_OFFSET,
                                                COLUMN_FORMAT_TEXT_SUBHEADER_INDEX_LENGTH) |> ToShort
                        TextSubHeaderLabel  = slice' page (offset +
                                                COLUMN_LABEL_TEXT_SUBHEADER_INDEX_OFFSET,
                                                COLUMN_LABEL_TEXT_SUBHEADER_INDEX_LENGTH) |> ToShort
                        ColumnFormatOffset  =  slice' page (offset +
                                                COLUMN_FORMAT_OFFSET_OFFSET,
                                                COLUMN_FORMAT_OFFSET_LENGTH) |> ToShort
                        ColumnFormatLength  = slice' page (offset +
                                                COLUMN_FORMAT_LENGTH_OFFSET,
                                                COLUMN_FORMAT_LENGTH_LENGTH) |> ToShort
                        ColumnLabelOffset   = slice' page (offset +
                                                COLUMN_LABEL_OFFSET_OFFSET,
                                                COLUMN_LABEL_OFFSET_LENGTH) |> ToShort
                        ColumnLabelLength   = slice' page (offset +
                                                COLUMN_LABEL_LENGTH_OFFSET,
                                                COLUMN_LABEL_LENGTH_LENGTH) |> ToShort
                       }
                    ColumnFormatLabel colFormatLabel
                | SubHeaderType.ColumnList ->
                    ColumnList
                | SubHeaderType.Data ->
                    DataPointer (subHeaderPointer, page)
                | SubHeaderType.Unknown -> failwith "Unknown subheader type"

            [0..subHeaderCount - 1]
            |> Seq.map readSubHeaderPointerBytes
            |> Seq.map readSubHeaderPointer
            |> Seq.map readSubHeader
            |> Seq.toList

        match pageType with
        | PAGE_META_TYPE -> Meta <| readSubHeaders()
        | PAGE_DATA_TYPE -> Data (blockCount, page)
        | PAGE_MIX_TYPE1
        | PAGE_MIX_TYPE2 -> Mix (readSubHeaders(), blockCount, page)
        | PAGE_AMD_TYPE
        | PAGE_METC_TYPE
        | PAGE_COMP_TYPE -> EmptyPage
        | _ ->
            dprintfn "Unknown page type: %i" pageType
            EmptyPage


    // collect subheaders from all pages and generate MetaData
    let meta =
        // helper function to only get pages that contain metadata (skip rest, big speedup for large files)
        let subHeaders =
            seq {
                // skip rest of header (header itself says how big it is)
                reader.BaseStream.Seek(int64 header.HeaderSize , SeekOrigin.Begin) |> ignore
                for n in [1..header.PageCount] do
                    let page = reader.ReadBytes header.PageSize
                    if page.Length < header.PageSize then
                        failwith "Couldn't read page"
                    yield readPage page }
            |> Seq.takeWhile (fun page ->
                match page with
                | Meta _
                | Mix _ -> true
                | _ -> false )
            |> Seq.collect (fun mp ->
                    match mp with
                    | Meta  subs         -> subs
                    | Mix  (subs , _, _) -> subs
                    | _ -> failwith "There should only be Meta or Mix here" )
            |> Seq.cache

        // only one of these
        let rowSize, rowCount, columnCount1, columnCount2, lcs, lcp =
            subHeaders
            |> Seq.pick (fun h ->
                match h with
                | Rows (rowSize, rowCount, col1, col2, lcs, lcp) ->
                    Some (rowSize, rowCount, col1, col2, lcs, lcp)
                | _ -> None
            )
        let columnCount =
            subHeaders
            |> Seq.pick (fun h ->
                match h with
                | ColumnCount n -> Some n
                | _ -> None
            )
        if columnCount <> columnCount1 + columnCount2 then
            dprintfn "Column count mismatch"

        // collect all headers of type ColumnText
        let textHeaders  =
            subHeaders
            |> Seq.choose (fun h ->
                match h with
                | ColumnText raw -> Some raw  // extract the bytes within
                | _ -> None )
            |> Seq.cache

        // compression information in the first ColumnText
        let compression =
            let firstColText = Seq.nth 0 textHeaders
            let offset = match header.Bits with X86 -> 16 | X64 -> 20
            let signature = slice firstColText (offset, 8) |> ToStr

            match signature.Trim() with
            | "" ->
               let creatorProc = slice firstColText (offset + 16, int lcp) |> ToStr
               Compression.NotCompressed creatorProc
            | RLE_COMPRESSION ->
                let creatorProc = slice firstColText (offset + 24, int lcp) |> ToStr
                Compression.RLE creatorProc
            | RDC_COMPRESSION -> Compression.RDC
            | _ -> Compression.NotCompressed ""

        let names =
            subHeaders
            |> Seq.choose (
                fun h ->
                match h with
                | ColumnName name -> Some name
                | _ -> None)
            |> Seq.concat

        let attributes =
            subHeaders
            |> Seq.choose (
                fun h ->
                match h with
                | ColumnAttributes attr -> Some attr
                | _ -> None)
            |> Seq.concat

        let formats =
            subHeaders
            |> Seq.choose (
                fun h ->
                match h with
                | ColumnFormatLabel format -> Some format
                | _ -> None)

        // reconstruct column info from the pieces that once must have been so close together o_O
        let columns =
            (names, attributes, formats)
            |||> Seq.zip3
            |> Seq.mapi (fun i (name, attr, format) ->
            let textHeader = Seq.nth (int name.TextIndex) textHeaders
            let textHeader = textHeader.[header.WordLength ..]

            // min used to prevent incorrect data which appear in some files
            let formatLen =
                min
                <| int format.ColumnFormatLength
                <| textHeader.Length - int format.ColumnFormatOffset
            let labelLen =
                min
                <| int format.ColumnLabelLength
                <| textHeader.Length - int format.ColumnLabelOffset
            {
                Ordinal = i + 1
                Name =
                    slice
                        textHeader
                        (int name.ColumnNameOffset, int name.ColumnNameLength)
                    |> ToStr
                Type =  match attr.ColumnType with
                        | 1uy -> Numeric
                        | 2uy -> Text
                        | _   -> failwith "Unknown ColumnType"
                Length = attr.ColumnAttrWidth
                Format =
                    slice
                        textHeader
                        (int format.ColumnFormatOffset, formatLen)
                    |> ToStr
                Label =
                    slice
                        textHeader
                        (int format.ColumnLabelOffset, labelLen)
                    |> ToStr
                Offset = attr.ColumnAttrOffset
            })
            |> Seq.toList

        {   RowCount = rowCount
            RowSize  = rowSize
            Columns  = columns
            CompressionInfo =  compression } // return collected metadata


    interface System.IDisposable with
        member x.Dispose() =
            reader.Close()

    member x.FileName = filename
    member x.Name = name
    member x.Header = header
    member x.MetaData = meta

    member x.Rows =
        seq {
            reader.BaseStream.Seek(int64 header.HeaderSize , SeekOrigin.Begin) |> ignore
            for n in [1 .. header.PageCount] do
                let page = reader.ReadBytes header.PageSize
                if page.Length < header.PageSize then
                    failwith "Couldn't read page"
                yield readPage page
        }
        |> Seq.map (fun page ->
            let readPageRows blocks offset len data =
                let readRow offset len =
                    let rowBytes = decompress meta offset len data
                    seq {
                    for n = 0 to List.length meta.Columns - 1 do
                        let col = meta.Columns.[n]
                        let colBytes = sliceEndian rowBytes (col.Offset, col.Length) header.Endianness
                        yield
                            match col.Type, col.Length, col.Format with
                            | Numeric, _, "" ->
                                    match ToDouble colBytes with
                                    | Some number -> Number number
                                    | None -> Empty
                            | Numeric, _, TIME_FORMAT_STRINGS ->
                                    match ToDateTime colBytes with
                                    | Some time -> Time time
                                    | None -> Empty
                            | Numeric, _, DATE_TIME_FORMAT_STRINGS ->
                                    match ToDateTime colBytes with
                                    | Some dtime -> DateAndTime dtime
                                    | None -> Empty
                            | Numeric, _, dt when List.exists
                                (fun fmt -> fmt = dt)
                                DATE_FORMAT_STRINGS ->
                                    match ToDate colBytes with
                                    | Some date -> Date date
                                    | None -> Empty
                            | Numeric, _, _ -> //todo: handle formats
                                    match ToDouble colBytes with
                                    | Some number -> Number number
                                    | None -> Empty
                            | Text, len, _ -> colBytes |> ToStr |> Character
                            | _ -> failwith "Couldn't parse value"
                        }
                Seq.init blocks (fun block -> offset + block*len )
                |> Seq.map (fun offset -> readRow offset len )

            match page with
            | Meta subHeaders ->
                subHeaders
                |> Seq.choose (
                    fun sub ->
                    match sub with
                    | DataPointer (pointer, data) -> Some (pointer, data)
                    | _ -> None)
                |> Seq.map (fun (pointer, data) ->
                    readPageRows 1 pointer.Offset pointer.Length data
                )
                |> Seq.concat
            | Mix (subHeaders, blockCount, data) ->
                let subCount = List.length subHeaders
                let align =
                    ( header.PageBitOffset +
                      SUBHEADER_POINTERS_OFFSET +
                      subCount*header.SubHeaderPointerLength
                    ) % 8

                readPageRows (blockCount - subCount)
                    ( align +
                      header.PageBitOffset +
                      SUBHEADER_POINTERS_OFFSET +
                      subCount*header.SubHeaderPointerLength )
                    meta.RowSize
                    data

            | Data (blockCount, data) ->
                readPageRows
                    blockCount
                    (header.PageBitOffset + SUBHEADER_POINTERS_OFFSET)
                    meta.RowSize
                    data
            | EmptyPage -> Seq.empty

            )
            |> Seq.concat