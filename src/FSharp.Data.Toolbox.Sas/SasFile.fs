namespace FSharp.Data.Toolbox.SasFile

//module SasFile =

open System
open System.Diagnostics
open System.IO

type SasFile (filename) = 

    let reader = 
        if not <| File.Exists filename then
            failwith "File '%s' not found" filename
        new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))

    let header = 
        let headerBytes = 
            let header = reader.ReadBytes META_LENGTH
            if header.Length < META_LENGTH then
                failwith "File is not SAS7BDAT (too short)"
            // Check magic number
            if header.[..Magic.Length - 1] <> Magic then
                Trace.WriteLine "Magic number mismatch"
                //failwith "Magic number mismatch"
            header

        let readHeader offset len = 
            headerBytes.[offset .. offset + len - 1]

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
            let dateCreatedBytes = readHeader (DATE_CREATED_OFFSET + alignment1) DATE_CREATED_LENGTH
            ToDate dateCreatedBytes
            
        let dateModified = 
            let dateModifiedBytes = readHeader (DATE_MODIFIED_OFFSET + alignment1) DATE_MODIFIED_LENGTH
            ToDate dateModifiedBytes

        let headerSize = 
            let headerSizeBytes = readHeader (HEADER_SIZE_OFFSET + alignment1) HEADER_SIZE_LENGTH
            let headerSize = ToInt headerSizeBytes
            if bits = X64 && headerSize <> 8*1024 then
                failwith "Header size (%d) doesn't match word length (X64)" headerSize
            headerSize

        let pageSize = 
            let pageSizeBytes = readHeader (PAGE_SIZE_OFFSET + alignment1) PAGE_SIZE_LENGTH
            ToInt pageSizeBytes

        let pageCount = 
            let pageCountBytes = readHeader (PAGE_COUNT_OFFSET + alignment1) PAGE_COUNT_LENGTH
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
            let osNameBytes = readHeader osNameOffset osNameLen
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
            OsName                 =  osName        
            Compression            =  NotCompressed }


    let readPage (page: byte array) = 

        let pageBytes offset len = 
            let offset = offset + header.PageBitOffset
            slice page (offset, len)

        let pageType = 
            let bytes = pageBytes PAGE_TYPE_OFFSET PAGE_TYPE_LENGTH
            ToShort bytes

        let blockCount = 
            let bytes = pageBytes BLOCK_COUNT_OFFSET BLOCK_COUNT_LENGTH
            ToShort bytes |> int


//        let (|Meta|Data|Mix|) pageType = 
//            if pageType = PAGE_META_TYPE then
//                Meta 
//            elif pageType = PAGE_DATA_TYPE then
//                Data 
//            elif pageType = PAGE_MIX_TYPE1 || pageType = PAGE_MIX_TYPE2 || pageType = PAGE_AMD_TYPE then
//                Mix 

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
                { Offset      = slice subBytes (0,          word) |> ToInt 
                  Length      = slice subBytes (word,       word) |> ToInt 
                  Compression = slice subBytes (2*word,     1)    |> ToByte
                  Type        = slice subBytes (2*word + 1, 1)    |> ToByte }

            let readSubHeader subHeaderPointer =
                let subHeaderType =
                    if subHeaderPointer.Compression = TRUNCATED_SUBHEADER_ID ||
                       subHeaderPointer.Length = 0 then SubHeaderType.Truncated
                    else
                        // read_subheader_signature
                        let subSignature = pageBytes (subHeaderPointer.Offset - header.PageBitOffset) header.WordLength
                        // get_subheader_class
                        let subHeaderMatched, subHeader = SignatureToHeaderColumn.TryGetValue subSignature
                //            if header.Compression <> NotCompressed && 
                //                not subHeaderMatched &&
                //                (subHeaderPointer.Compression = COMPRESSED_SUBHEADER_ID ||
                //                 subHeaderPointer.Compression = 0uy) &&
                //                 subHeaderPointer.Type = COMPRESSED_SUBHEADER_TYPE then
                //                 Data
                //            else 
                        if subHeaderMatched then subHeader
                        else SubHeaderType.Unknown

                let offset = subHeaderPointer.Offset
                let word = header.WordLength

                let lcsOffset = offset + if header.Bits = X64 then 682 else 354
                let lcpOffset = offset + if header.Bits = X64 then 706 else 378
                let rowCountMix = 
                    slice page (offset + ROW_COUNT_ON_MIX_PAGE_OFFSET_MULTIPLIER*word, word)
                    |> ToInt


                let lcs = slice page (lcsOffset, 2) |> ToShort
                let lcp = slice page (lcpOffset, 2) |> ToShort

                match subHeaderType with
                | SubHeaderType.Truncated-> Truncated
                | SubHeaderType.Rows ->
                    let rowLength = 
                        slice page (offset + ROW_LENGTH_OFFSET_MULTIPLIER*word, word)
                        |> ToInt
                    let rowCount = 
                        slice page (offset + ROW_COUNT_OFFSET_MULTIPLIER*word, word)
                        |> ToInt

                    Rows (rowLength, rowCount) 
                | SubHeaderType.ColumnCount -> 
                    let colCountP1 = 
                        slice page (offset + COL_COUNT_P1_MULTIPLIER*word, word)
                        |> ToInt

                    let colCountP2 = 
                        slice page (offset + COL_COUNT_P2_MULTIPLIER*word, word)
                        |> ToInt

                    let offset = offset + word
                    let colCount = slice page (offset, word) |> ToInt
                    if colCount <> colCountP1 + colCountP2 then
                        Trace.WriteLine "Column count mismatch"
                    ColumnCount colCount
                | SubHeaderType.SubHeaderCount ->
                    SubHeaderCounts // Not sure what to do here yet
                | SubHeaderType.ColumnText ->
                    let offset = offset + word
                    let textBlockSize = slice page (offset, TEXT_BLOCK_SIZE_LENGTH) |> ToShort
                    let columnNames = slice page (offset, int textBlockSize)
                    ColumnText columnNames
                | SubHeaderType.ColumnName ->
                    let offset = offset + word
                    let count = (subHeaderPointer.Length - 2*word - 12) / 8
                    let names = 
                        [1 .. count]
                        |> List.map (fun n -> 
                            let offset = offset + COLUMN_NAME_POINTER_LENGTH*n 
                            {
                            TextIndex        = slice page (offset + 
                                                COLUMN_NAME_TEXT_SUBHEADER_OFFSET, 
                                                COLUMN_NAME_TEXT_SUBHEADER_LENGTH) |> ToShort
                            ColumnNameOffset = slice page (offset + 
                                                COLUMN_NAME_OFFSET_OFFSET,
                                                COLUMN_NAME_OFFSET_LENGTH) |> ToShort 
                            ColumnNameLength = slice page (offset + 
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
                            ColumnAttrOffset = slice page (offset + 
                                                COLUMN_DATA_OFFSET_OFFSET, 
                                                word) |> ToInt
                            ColumnAttrWidth  = slice page (offset + 
                                                word + COLUMN_DATA_LENGTH_OFFSET,
                                                COLUMN_DATA_LENGTH_LENGTH) |> ToInt 
                            ColumnType       = slice page (offset + 
                                                word + COLUMN_TYPE_OFFSET,
                                                COLUMN_TYPE_LENGTH) |> ToByte
                            })               
                    ColumnAttributes attributes
                | SubHeaderType.ColumnFormatLabel ->
                    let offset = subHeaderPointer.Offset + 3*word
                    let colFormatLabel = 
                        {
                        TextSubHeaderFormat = slice page (offset + 
                                                COLUMN_FORMAT_TEXT_SUBHEADER_INDEX_OFFSET,
                                                COLUMN_FORMAT_TEXT_SUBHEADER_INDEX_LENGTH) |> ToShort
                        TextSubHeaderLabel  = slice page (offset + 
                                                COLUMN_LABEL_TEXT_SUBHEADER_INDEX_OFFSET,
                                                COLUMN_LABEL_TEXT_SUBHEADER_INDEX_LENGTH) |> ToShort
                        ColumnFormatOffset  =  slice page (offset + 
                                                COLUMN_FORMAT_OFFSET_OFFSET,
                                                COLUMN_FORMAT_OFFSET_LENGTH) |> ToShort
                        ColumnFormatLength  = slice page (offset + 
                                                COLUMN_FORMAT_LENGTH_OFFSET,
                                                COLUMN_FORMAT_LENGTH_LENGTH) |> ToShort
                        ColumnLabelOffset   = slice page (offset + 
                                                COLUMN_LABEL_OFFSET_OFFSET,
                                                COLUMN_LABEL_OFFSET_LENGTH) |> ToShort
                        ColumnLabelLength   = slice page (offset + 
                                                COLUMN_LABEL_LENGTH_OFFSET,
                                                COLUMN_LABEL_LENGTH_LENGTH) |> ToShort
                       }
                    ColumnFormatLabel colFormatLabel
                | SubHeaderType.ColumnList ->
                    ColumnList
                | SubHeaderType.Unknown -> failwith "Unknown subheader type"
                    
            [0..subHeaderCount - 1] 
            |> List.map readSubHeaderPointerBytes 
            |> List.map readSubHeaderPointer
            |> List.map readSubHeader 
            

        
        match pageType with
        | PAGE_META_TYPE -> readSubHeaders() |> Meta
        | PAGE_DATA_TYPE -> Data (blockCount, page)
        | PAGE_MIX_TYPE1
        | PAGE_MIX_TYPE2 -> Mix (readSubHeaders(), blockCount, page)
        | PAGE_AMD_TYPE  -> AMD (readSubHeaders(), blockCount, page)
        | _ -> failwith "Unknown page type"


    // collect subheaders from all pages into one list
    let meta =
        // helper function to only get pages that contain metadata (skip rest, big speedup for large files) 
        let metaPages =  
            seq {
                // skip rest of header (header itself says how big it is)
                reader.BaseStream.Position <- int64 header.HeaderSize 
                for n in [1..header.PageCount] do
                    let page = reader.ReadBytes header.PageSize
                    if page.Length < header.PageSize then
                        failwith "Couldn't read page"
                    yield readPage page
            }
            |> Seq.takeWhile (fun page -> 
                match page with 
                | Meta _ 
                | Mix _ 
                | AMD _ -> true 
                | _ -> false
            )

        // collect subheaders from all pages with metadata
        let subHeaders = 
            seq {    
            for mp in metaPages do
                match mp with 
                | Meta subHeaders -> yield! subHeaders
                | Mix  (subHeaders, _, _) -> yield! subHeaders
                | AMD  (subHeaders, _, _) -> yield! subHeaders
                | _ -> failwith "There should only be Meta or Mix here"                
            } |> Seq.toList
                   
        // only one of these                
        let rowSize, rowCount =
            subHeaders
            |> List.pick (fun h ->
                match h with 
                | Rows (rowSize, rowCount) -> Some (rowSize, rowCount)
                | _-> None
            )        

        // collect all headers of type ColumnText
        let textHeaders  =      
            subHeaders
            |> List.choose (fun h ->
                match h with 
                | ColumnText raw -> Some raw  // extract the bytes within
                | _-> None
            )        
        
        let names = 
            subHeaders
            |> List.choose (
                fun h -> 
                match h with 
                | ColumnName n  -> Some n
                | _ -> None)
            |> List.concat 

        let attributes = 
            subHeaders
            |> List.choose (
                fun h -> 
                match h with 
                | ColumnAttributes attr -> Some attr
                | _ -> None)
            |> List.concat    

        let formats = 
            subHeaders
            |> List.choose (
                fun h -> 
                match h with 
                | ColumnFormatLabel format -> Some format
                | _ -> None)

        let columns = 
            (names, attributes, formats) // reconstruct column info from the pieces that once must have been so close together o_O
            |||> List.zip3
            |> List.mapi (fun i (name, attr, format) -> 
            {
                Ordinal = i
                Name = 
                    slice 
                        (List.nth textHeaders (int name.TextIndex) )
                        (int name.ColumnNameOffset, int name.ColumnNameLength) 
                    |> ToStr
                Type =  match attr.ColumnType with 
                        | 1uy -> Numeric
                        | 2uy -> Text
                        | _   -> failwith "Unknown ColumnType"
                Length = attr.ColumnAttrWidth
                Format = 
                    slice 
                        (List.nth textHeaders (int name.TextIndex) )
                        (int format.ColumnFormatOffset, int format.ColumnFormatLength) 
                    |> ToStr
                Label = 
                    slice 
                        (List.nth textHeaders (int name.TextIndex) )
                        (int format.ColumnLabelOffset, int format.ColumnLabelLength) 
                    |> ToStr
                Offset = attr.ColumnAttrOffset
            })

        {   RowCount = rowCount
            RowSize  = rowSize
            Columns  = columns   } // return collected metadata


    interface System.IDisposable with 
        member x.Dispose() = 
            reader.Close()

    member x.FileName = filename
    member x.Header = header
    member x.MetaData = meta

    
    member x.ReadLines() =
        seq {
            // skip header (header itself says how big it is)
            reader.BaseStream.Position <- int64 header.HeaderSize 
            for n in [1..header.PageCount] do
                let page = reader.ReadBytes header.PageSize
                if page.Length < header.PageSize then
                    failwith "Couldn't read page"
                yield readPage page
        }
        |> Seq.skipWhile (fun page ->
            match page with | Meta _ -> true | _ -> false)
        |> Seq.map (fun page ->
            match page with
            | Mix (subHeaders, blockCount, data)
            | AMD (subHeaders, blockCount, data) ->
                let subCount = List.length subHeaders
                let align = 
                    (header.PageBitOffset + 
                     SUBHEADER_POINTERS_OFFSET + subCount*header.SubHeaderPointerLength
                     ) % 8
                let offset = 
                    header.PageBitOffset + 
                    SUBHEADER_POINTERS_OFFSET + align + 
                    subCount*header.SubHeaderPointerLength + 
                    1(*rowNumber*) *meta.RowSize
                ()

                )



//            [1 .. meta.RowCount]
//            |> List.map (fun n -> 
//                
//                )



