namespace FSharp.Data.Toolbox.Sas

[<AutoOpen>]
module SasSignatures =

    /// SAS7BDAT file signature
    let Magic= FromHex """
        00-00-00-00-00
        00-00-00-00-00
        00-00-C2-EA-81
        60-B3-14-11-CF
        BD-92-08-00-09
        C7-31-8C-18-1F
        10-11"""

    let META_LENGTH = 288
    let ALIGN_1_CHECKER_VALUE = "3"B
    let ALIGN_1_OFFSET = 32
    let ALIGN_1_LENGTH = 1
    let ALIGN_1_VALUE = 4
    let U64_BYTE_CHECKER_VALUE = "3"B
    let ALIGN_2_OFFSET = 35
    let ALIGN_2_LENGTH = 1
    let ALIGN_2_VALUE = 4
    let ENDIANNESS_OFFSET = 37
    let ENDIANNESS_LENGTH = 1
    let PLATFORM_OFFSET = 39
    let PLATFORM_LENGTH = 1
    let DATASET_OFFSET = 92
    let DATASET_LENGTH = 64
    let FILE_TYPE_OFFSET = 156
    let FILE_TYPE_LENGTH = 8
    let DATE_CREATED_OFFSET = 164
    let DATE_CREATED_LENGTH = 8
    let DATE_MODIFIED_OFFSET = 172
    let DATE_MODIFIED_LENGTH = 8
    let HEADER_SIZE_OFFSET = 196
    let HEADER_SIZE_LENGTH = 4
    let PAGE_SIZE_OFFSET = 200
    let PAGE_SIZE_LENGTH = 4
    let PAGE_COUNT_OFFSET = 204
    let PAGE_COUNT_LENGTH = 4
    let SAS_RELEASE_OFFSET = 216
    let SAS_RELEASE_LENGTH = 8
    let SAS_SERVER_TYPE_OFFSET = 224
    let SAS_SERVER_TYPE_LENGTH = 16
    let OS_VERSION_NUMBER_OFFSET = 240
    let OS_VERSION_NUMBER_LENGTH = 16
    let OS_MAKER_OFFSET = 256
    let OS_MAKER_LENGTH = 16
    let OS_NAME_OFFSET = 272
    let OS_NAME_LENGTH = 16
    let PAGE_BIT_OFFSET_X86 = 16
    let PAGE_BIT_OFFSET_X64 = 32
    let WORD_LENGTH_X86 = 4
    let WORD_LENGTH_X64 = 8
    let SUBHEADER_POINTER_LENGTH_X86 = 12
    let SUBHEADER_POINTER_LENGTH_X64 = 24
    let PAGE_TYPE_OFFSET = 0
    let PAGE_TYPE_LENGTH = 2
    let BLOCK_COUNT_OFFSET = 2
    let BLOCK_COUNT_LENGTH = 2
    let SUBHEADER_COUNT_OFFSET = 4
    let SUBHEADER_COUNT_LENGTH = 2

    //let PAGE_META_TYPE = set [ 0s ]
    //let PAGE_DATA_TYPE = set [ 256s ]
    //let PAGE_MIX_TYPE = set [512s; 640s]
    //let PAGE_AMD_TYPE = set [ 1024s ]
    //let PAGE_METC_TYPE = set [ 16384s ]
    //let PAGE_COMP_TYPE = set [ -28672s ]
    //let PAGE_MIX_DATA_TYPE = PAGE_DATA_TYPE + PAGE_MIX_TYPE
    //let PAGE_META_MIX_AMD = PAGE_META_TYPE + PAGE_AMD_TYPE + PAGE_MIX_TYPE
    //let PAGE_ANY = PAGE_DATA_TYPE + PAGE_METC_TYPE + PAGE_COMP_TYPE + PAGE_META_MIX_AMD

    [<Literal>]
    let PAGE_META_TYPE = 0s
    [<Literal>]
    let PAGE_DATA_TYPE = 256s
    [<Literal>]
    let PAGE_MIX_TYPE1 = 512s
    [<Literal>]
    let PAGE_MIX_TYPE2 = 640s
    [<Literal>]
    let PAGE_AMD_TYPE  = 1024s
    [<Literal>]
    let PAGE_METC_TYPE = 16384s
    [<Literal>]
    let PAGE_COMP_TYPE = -28672s
    //let ??? = -32768s

    let SUBHEADER_POINTERS_OFFSET = 8
    let TRUNCATED_SUBHEADER_ID = 1uy
    let COMPRESSED_SUBHEADER_ID = 4uy
    let COMPRESSED_SUBHEADER_TYPE = 1uy

    // Subheader signatures, 32 and 64 bit, little and big endian
    let SignatureToHeaderColumn =
        dict [
            FromHex "F7F7F7F7",         SubHeaderType.Rows
            FromHex "00000000F7F7F7F7", SubHeaderType.Rows
            FromHex "F7F7F7F700000000", SubHeaderType.Rows
            FromHex "F6F6F6F6",         SubHeaderType.ColumnCount
            FromHex "00000000F6F6F6F6", SubHeaderType.ColumnCount
            FromHex "F6F6F6F600000000", SubHeaderType.ColumnCount
            FromHex "00FCFFFF",         SubHeaderType.SubHeaderCount
            FromHex "FFFFFC00",         SubHeaderType.SubHeaderCount
            FromHex "00FCFFFFFFFFFFFF", SubHeaderType.SubHeaderCount
            FromHex "FFFFFFFFFFFFFC00", SubHeaderType.SubHeaderCount
            FromHex "FDFFFFFF",         SubHeaderType.ColumnText
            FromHex "FFFFFFFD",         SubHeaderType.ColumnText
            FromHex "FDFFFFFFFFFFFFFF", SubHeaderType.ColumnText
            FromHex "FFFFFFFFFFFFFFFD", SubHeaderType.ColumnText
            FromHex "FFFFFFFF",         SubHeaderType.ColumnName
            FromHex "FFFFFFFFFFFFFFFF", SubHeaderType.ColumnName
            FromHex "FCFFFFFF",         SubHeaderType.ColumnAttributes
            FromHex "FFFFFFFC",         SubHeaderType.ColumnAttributes
            FromHex "FCFFFFFFFFFFFFFF", SubHeaderType.ColumnAttributes
            FromHex "FFFFFFFFFFFFFFFC", SubHeaderType.ColumnAttributes
            FromHex "FEFBFFFF",         SubHeaderType.ColumnFormatLabel
            FromHex "FFFFFBFE",         SubHeaderType.ColumnFormatLabel
            FromHex "FEFBFFFFFFFFFFFF", SubHeaderType.ColumnFormatLabel
            FromHex "FFFFFFFFFFFFFBFE", SubHeaderType.ColumnFormatLabel
            FromHex "FEFFFFFF",         SubHeaderType.ColumnList
            FromHex "FFFFFFFE",         SubHeaderType.ColumnList
            FromHex "FEFFFFFFFFFFFFFF", SubHeaderType.ColumnList
            FromHex "FFFFFFFFFFFFFFFE", SubHeaderType.ColumnList
        ]

    let TEXT_BLOCK_SIZE_LENGTH = 2
    let ROW_LENGTH_OFFSET_MULTIPLIER = 5
    let ROW_COUNT_OFFSET_MULTIPLIER = 6
    let COL_COUNT_P1_MULTIPLIER = 9
    let COL_COUNT_P2_MULTIPLIER = 10
    let ROW_COUNT_ON_MIX_PAGE_OFFSET_MULTIPLIER = 15
    let COLUMN_NAME_POINTER_LENGTH = 8
    let COLUMN_NAME_TEXT_SUBHEADER_OFFSET = 0
    let COLUMN_NAME_TEXT_SUBHEADER_LENGTH = 2
    let COLUMN_NAME_OFFSET_OFFSET = 2
    let COLUMN_NAME_OFFSET_LENGTH = 2
    let COLUMN_NAME_LENGTH_OFFSET = 4
    let COLUMN_NAME_LENGTH_LENGTH = 2
    let COLUMN_DATA_OFFSET_OFFSET = 8
    let COLUMN_DATA_LENGTH_OFFSET = 8
    let COLUMN_DATA_LENGTH_LENGTH = 4
    let COLUMN_TYPE_OFFSET = 14
    let COLUMN_TYPE_LENGTH = 1
    let COLUMN_FORMAT_TEXT_SUBHEADER_INDEX_OFFSET = 22
    let COLUMN_FORMAT_TEXT_SUBHEADER_INDEX_LENGTH = 2
    let COLUMN_FORMAT_OFFSET_OFFSET = 24
    let COLUMN_FORMAT_OFFSET_LENGTH = 2
    let COLUMN_FORMAT_LENGTH_OFFSET = 26
    let COLUMN_FORMAT_LENGTH_LENGTH = 2
    let COLUMN_LABEL_TEXT_SUBHEADER_INDEX_OFFSET = 28
    let COLUMN_LABEL_TEXT_SUBHEADER_INDEX_LENGTH = 2
    let COLUMN_LABEL_OFFSET_OFFSET = 30
    let COLUMN_LABEL_OFFSET_LENGTH = 2
    let COLUMN_LABEL_LENGTH_OFFSET = 32
    let COLUMN_LABEL_LENGTH_LENGTH = 2

    [<Literal>]
    let TIME_FORMAT_STRINGS = "TIME"
    [<Literal>]
    let DATE_TIME_FORMAT_STRINGS = "DATETIME"
    let DATE_FORMAT_STRINGS = ["YYMMDD"; "MMDDYY"; "DDMMYY"; "DATE"; "JULIAN"; "MONYY"]

    [<Literal>]
    let RLE_COMPRESSION = "SASYZCRL"
    [<Literal>]
    let RDC_COMPRESSION = "SASYZCR2"
