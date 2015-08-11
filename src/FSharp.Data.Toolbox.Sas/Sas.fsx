#I @"..\..\bin"
#r "FSharp.Data.Toolbox.Sas"

open System
open System.IO
open System.Diagnostics

open FSharp.Data.Toolbox.Sas.Core
open FSharp.Data.Toolbox.Sas.SasSignatures
open FSharp.Data.Toolbox.Sas.SasFile

let path = Path.Combine(Directory.GetParent(__SOURCE_DIRECTORY__).Parent.FullName, @"tests\FSharp.Data.Toolbox.Sas.Tests\files\acadindx.sas7bdat")

let reader = 
    if not <| File.Exists path then
        failwith "File '%s' not found" path
    new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))

let header = 
    let headerBytes = 
        let header = reader.ReadBytes META_LENGTH
        if header.Length < META_LENGTH then
            failwith "File is not SAS7BDAT (too short)"
        // Check magic number
        if header.[..Magic.Length - 1] <> Magic then
            Trace.WriteLine "Magic number mismatch"
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
        | _    -> Unknown

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



reader.BaseStream.Position <- int64 header.HeaderSize 
//for n in [1..header.PageCount] do
    //let pos = header.PageSize * n |> int64 
let bytes = reader.ReadBytes header.PageSize
if bytes.Length < header.PageSize then
    failwith "Couldn't read page"
    //yield readPage bytes

    // process_page_metadata
    //let readMeta () =

let page = bytes //>>
let pageBytes offset len = 
    let offset = offset + header.PageBitOffset
    page.[offset .. offset + len - 1]

let pageType = 
    let bytes = pageBytes PAGE_TYPE_OFFSET PAGE_TYPE_LENGTH
    ToShort bytes

let blockCount = 
    let bytes = pageBytes BLOCK_COUNT_OFFSET BLOCK_COUNT_LENGTH
    ToShort bytes |> int

let subHeaderCount = 
    let bytes = pageBytes SUBHEADER_COUNT_OFFSET SUBHEADER_COUNT_LENGTH
    ToShort bytes |> int


let n = 0
let subHeaderPointerBytes n = 
    let subHeaderPointerOffset = SUBHEADER_POINTERS_OFFSET + header.PageBitOffset
    let totalOffset = subHeaderPointerOffset + n * header.SubHeaderPointerLength
    page.[totalOffset .. totalOffset + 2 * header.WordLength + 2],
    totalOffset 
    
// process_subheader_pointers
let subHeaderPointer (bytes: byte array, totalOffset) =
    let word = header.WordLength
    { Offset      = bytes.[0          ..   word - 1] |> ToInt 
      Length      = bytes.[word       .. 2*word - 1] |> ToInt 
      Compression = bytes.[2*word     .. 2*word + 1].[0] 
      Type        = bytes.[2*word + 1 .. 2*word + 2].[0] }



let subHeader subHeaderPointer =
    let subHeader =
        if subHeaderPointer.Compression = TRUNCATED_SUBHEADER_ID then Truncated
        else
            // read_subheader_signature
            let subSignature = pageBytes subHeaderPointer.Offset header.WordLength
            // get_subheader_class
            let subHeaderMatched, subHeader = SignatureToHeaderColumn.TryGetValue subSignature
            //let subHeader = 
            if header.Compression <> NotCompressed && 
                not subHeaderMatched &&
                (subHeaderPointer.Compression = COMPRESSED_SUBHEADER_ID ||
                 subHeaderPointer.Compression = 0uy) &&
                 subHeaderPointer.Type = COMPRESSED_SUBHEADER_TYPE then
                 Data
            else subHeader
    subHeader, subHeaderPointer

        
let readSubHeader = function
| RowSize, subHeaderPointer ->
    let offset = subHeaderPointer.Offset
    let word = header.WordLength
    let lcsOffset = offset + if header.Bits = X64 then 682 else 354
    let lcpOffset = offset + if header.Bits = X64 then 706 else 378
    let rowLengthOffsetMultiplier = page.[offset + ROW_LENGTH_OFFSET_MULTIPLIER * word .. word]
    let rowCountOffsetMultiplier = page.[offset + ROW_COUNT_OFFSET_MULTIPLIER * word .. word]
    let rowCountMixOffsetMultiplier = page.[offset + ROW_COUNT_ON_MIX_PAGE_OFFSET_MULTIPLIER * word .. word]
    let colCountP1OffsetMultiplier = page.[offset + COL_COUNT_P1_MULTIPLIER * word .. word]
    let colCountP2OffsetMultiplier = page.[offset + COL_COUNT_P2_MULTIPLIER * word .. word]
    let lcs = page.[lcsOffset .. 2]
    let lcp = page.[lcpOffset .. 2]
    ()


reader.Close()
