
namespace SasLib 

    open SAS.MetaData
    open System.IO
    open System
        
    type APPLICAN = {
        ``ID`` : float
        ``FL`` : float
        ``APP`` : float
        ``AA`` : float
        ``LA`` : float
        ``SC`` : float
        ``LC`` : float
        ``HON`` : float
        ``SMS`` : float
        ``EXP`` : float
        ``DRV`` : float
        ``AMB`` : float
        ``GSP`` : float
        ``POT`` : float
        ``KJ`` : float
        ``SUIT`` : float

    }
    with 
        static member rowSize = 128
        static member fromRaw (raw:byte[]) = {
                ``ID`` = BitConverter.ToDouble(raw, 0)
                ``FL`` = BitConverter.ToDouble(raw, 8)
                ``APP`` = BitConverter.ToDouble(raw, 16)
                ``AA`` = BitConverter.ToDouble(raw, 24)
                ``LA`` = BitConverter.ToDouble(raw, 32)
                ``SC`` = BitConverter.ToDouble(raw, 40)
                ``LC`` = BitConverter.ToDouble(raw, 48)
                ``HON`` = BitConverter.ToDouble(raw, 56)
                ``SMS`` = BitConverter.ToDouble(raw, 64)
                ``EXP`` = BitConverter.ToDouble(raw, 72)
                ``DRV`` = BitConverter.ToDouble(raw, 80)
                ``AMB`` = BitConverter.ToDouble(raw, 88)
                ``GSP`` = BitConverter.ToDouble(raw, 96)
                ``POT`` = BitConverter.ToDouble(raw, 104)
                ``KJ`` = BitConverter.ToDouble(raw, 112)
                ``SUIT`` = BitConverter.ToDouble(raw, 120)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*APPLICAN.rowSize .. dataStart+(i+1)*APPLICAN.rowSize-1]) // chop page 
                |> Array.map APPLICAN.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*APPLICAN.rowSize .. dataStart+(i+1)*APPLICAN.rowSize-1]) // chop page 
                |> Array.map APPLICAN.fromRaw
            | _ -> Array.empty<APPLICAN>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\applican.sas7bdat"
            for pp in p do
                yield! APPLICAN.p2a pp
        }    

    type BANK8DTE = {
        ``ACQUIRE`` : float
        ``ATMCT`` : float
        ``ADBDDA`` : float
        ``DDATOT`` : float
        ``DDADEP`` : float
        ``INCOME`` : float
        ``INVEST`` : float
        ``ATRES`` : float
        ``SAVBAL`` : float

    }
    with 
        static member rowSize = 72
        static member fromRaw (raw:byte[]) = {
                ``ACQUIRE`` = BitConverter.ToDouble(raw, 0)
                ``ATMCT`` = BitConverter.ToDouble(raw, 8)
                ``ADBDDA`` = BitConverter.ToDouble(raw, 16)
                ``DDATOT`` = BitConverter.ToDouble(raw, 24)
                ``DDADEP`` = BitConverter.ToDouble(raw, 32)
                ``INCOME`` = BitConverter.ToDouble(raw, 40)
                ``INVEST`` = BitConverter.ToDouble(raw, 48)
                ``ATRES`` = BitConverter.ToDouble(raw, 56)
                ``SAVBAL`` = BitConverter.ToDouble(raw, 64)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*BANK8DTE.rowSize .. dataStart+(i+1)*BANK8DTE.rowSize-1]) // chop page 
                |> Array.map BANK8DTE.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*BANK8DTE.rowSize .. dataStart+(i+1)*BANK8DTE.rowSize-1]) // chop page 
                |> Array.map BANK8DTE.fromRaw
            | _ -> Array.empty<BANK8DTE>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\bank8dte.sas7bdat"
            for pp in p do
                yield! BANK8DTE.p2a pp
        }    

    type BANK8DTR = {
        ``ACQUIRE`` : float
        ``ATMCT`` : float
        ``ADBDDA`` : float
        ``DDATOT`` : float
        ``DDADEP`` : float
        ``INCOME`` : float
        ``INVEST`` : float
        ``ATRES`` : float
        ``SAVBAL`` : float

    }
    with 
        static member rowSize = 72
        static member fromRaw (raw:byte[]) = {
                ``ACQUIRE`` = BitConverter.ToDouble(raw, 0)
                ``ATMCT`` = BitConverter.ToDouble(raw, 8)
                ``ADBDDA`` = BitConverter.ToDouble(raw, 16)
                ``DDATOT`` = BitConverter.ToDouble(raw, 24)
                ``DDADEP`` = BitConverter.ToDouble(raw, 32)
                ``INCOME`` = BitConverter.ToDouble(raw, 40)
                ``INVEST`` = BitConverter.ToDouble(raw, 48)
                ``ATRES`` = BitConverter.ToDouble(raw, 56)
                ``SAVBAL`` = BitConverter.ToDouble(raw, 64)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*BANK8DTR.rowSize .. dataStart+(i+1)*BANK8DTR.rowSize-1]) // chop page 
                |> Array.map BANK8DTR.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*BANK8DTR.rowSize .. dataStart+(i+1)*BANK8DTR.rowSize-1]) // chop page 
                |> Array.map BANK8DTR.fromRaw
            | _ -> Array.empty<BANK8DTR>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\bank8dtr.sas7bdat"
            for pp in p do
                yield! BANK8DTR.p2a pp
        }    

    type BEEF = {
        ``TRT`` : string
        ``REP`` : float
        ``STORAGE`` : float
        ``BEEFY`` : float
        ``BLOODY`` : float
        ``METAL`` : float
        ``GRASSY`` : float
        ``SOUR`` : float
        ``SPOILED`` : float

    }
    with 
        static member rowSize = 72
        static member fromRaw (raw:byte[]) = {
                ``TRT`` = byte2str raw 64 3
                ``REP`` = BitConverter.ToDouble(raw, 0)
                ``STORAGE`` = BitConverter.ToDouble(raw, 8)
                ``BEEFY`` = BitConverter.ToDouble(raw, 16)
                ``BLOODY`` = BitConverter.ToDouble(raw, 24)
                ``METAL`` = BitConverter.ToDouble(raw, 32)
                ``GRASSY`` = BitConverter.ToDouble(raw, 40)
                ``SOUR`` = BitConverter.ToDouble(raw, 48)
                ``SPOILED`` = BitConverter.ToDouble(raw, 56)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*BEEF.rowSize .. dataStart+(i+1)*BEEF.rowSize-1]) // chop page 
                |> Array.map BEEF.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*BEEF.rowSize .. dataStart+(i+1)*BEEF.rowSize-1]) // chop page 
                |> Array.map BEEF.fromRaw
            | _ -> Array.empty<BEEF>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\beef.sas7bdat"
            for pp in p do
                yield! BEEF.p2a pp
        }    

    type BIG8 = {
        ``SCHOOL`` : string
        ``GAMES`` : float
        ``RO_YDS`` : float
        ``RD_YDS`` : float
        ``PO_YDS`` : float
        ``PD_RAT`` : float
        ``TO_YDS`` : float
        ``TD_YDS`` : float
        ``SO`` : float
        ``SD`` : float
        ``TOM`` : float
        ``WINS`` : float

    }
    with 
        static member rowSize = 104
        static member fromRaw (raw:byte[]) = {
                ``SCHOOL`` = byte2str raw 88 14
                ``GAMES`` = BitConverter.ToDouble(raw, 0)
                ``RO_YDS`` = BitConverter.ToDouble(raw, 8)
                ``RD_YDS`` = BitConverter.ToDouble(raw, 16)
                ``PO_YDS`` = BitConverter.ToDouble(raw, 24)
                ``PD_RAT`` = BitConverter.ToDouble(raw, 32)
                ``TO_YDS`` = BitConverter.ToDouble(raw, 40)
                ``TD_YDS`` = BitConverter.ToDouble(raw, 48)
                ``SO`` = BitConverter.ToDouble(raw, 56)
                ``SD`` = BitConverter.ToDouble(raw, 64)
                ``TOM`` = BitConverter.ToDouble(raw, 72)
                ``WINS`` = BitConverter.ToDouble(raw, 80)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*BIG8.rowSize .. dataStart+(i+1)*BIG8.rowSize-1]) // chop page 
                |> Array.map BIG8.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*BIG8.rowSize .. dataStart+(i+1)*BIG8.rowSize-1]) // chop page 
                |> Array.map BIG8.fromRaw
            | _ -> Array.empty<BIG8>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\big8.sas7bdat"
            for pp in p do
                yield! BIG8.p2a pp
        }    

    type CARS = {
        ``make`` : string
        ``make_mod`` : string
        ``price`` : float
        ``et`` : string
        ``hp`` : float
        ``tim1`` : float
        ``tim2`` : float
        ``ts`` : float
        ``brak1`` : float
        ``brak2`` : float
        ``sp`` : float
        ``ss`` : float
        ``mpg`` : float

    }
    with 
        static member rowSize = 128
        static member fromRaw (raw:byte[]) = {
                ``make`` = byte2str raw 80 9
                ``make_mod`` = byte2str raw 89 24
                ``price`` = BitConverter.ToDouble(raw, 0)
                ``et`` = byte2str raw 113 8
                ``hp`` = BitConverter.ToDouble(raw, 8)
                ``tim1`` = BitConverter.ToDouble(raw, 16)
                ``tim2`` = BitConverter.ToDouble(raw, 24)
                ``ts`` = BitConverter.ToDouble(raw, 32)
                ``brak1`` = BitConverter.ToDouble(raw, 40)
                ``brak2`` = BitConverter.ToDouble(raw, 48)
                ``sp`` = BitConverter.ToDouble(raw, 56)
                ``ss`` = BitConverter.ToDouble(raw, 64)
                ``mpg`` = BitConverter.ToDouble(raw, 72)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*CARS.rowSize .. dataStart+(i+1)*CARS.rowSize-1]) // chop page 
                |> Array.map CARS.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*CARS.rowSize .. dataStart+(i+1)*CARS.rowSize-1]) // chop page 
                |> Array.map CARS.fromRaw
            | _ -> Array.empty<CARS>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\cars.sas7bdat"
            for pp in p do
                yield! CARS.p2a pp
        }    

    type CITIES1 = {
        ``CITY`` : string
        ``HIGH`` : float
        ``LOW`` : float

    }
    with 
        static member rowSize = 32
        static member fromRaw (raw:byte[]) = {
                ``CITY`` = byte2str raw 16 16
                ``HIGH`` = BitConverter.ToDouble(raw, 0)
                ``LOW`` = BitConverter.ToDouble(raw, 8)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*CITIES1.rowSize .. dataStart+(i+1)*CITIES1.rowSize-1]) // chop page 
                |> Array.map CITIES1.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*CITIES1.rowSize .. dataStart+(i+1)*CITIES1.rowSize-1]) // chop page 
                |> Array.map CITIES1.fromRaw
            | _ -> Array.empty<CITIES1>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\cities1.sas7bdat"
            for pp in p do
                yield! CITIES1.p2a pp
        }    

    type CITIES2 = {
        ``CITY`` : string
        ``HIGH`` : float
        ``LOW`` : float
        ``NORMAL_H`` : float

    }
    with 
        static member rowSize = 40
        static member fromRaw (raw:byte[]) = {
                ``CITY`` = byte2str raw 24 14
                ``HIGH`` = BitConverter.ToDouble(raw, 0)
                ``LOW`` = BitConverter.ToDouble(raw, 8)
                ``NORMAL_H`` = BitConverter.ToDouble(raw, 16)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*CITIES2.rowSize .. dataStart+(i+1)*CITIES2.rowSize-1]) // chop page 
                |> Array.map CITIES2.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*CITIES2.rowSize .. dataStart+(i+1)*CITIES2.rowSize-1]) // chop page 
                |> Array.map CITIES2.fromRaw
            | _ -> Array.empty<CITIES2>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\cities2.sas7bdat"
            for pp in p do
                yield! CITIES2.p2a pp
        }    

    type COOKIE = {
        ``__CKG`` : float
        ``HOLES`` : float
        ``PROTUS`` : float
        ``CONTOUR`` : float
        ``CELLSIZE`` : float
        ``CELLUNIF`` : float
        ``SURF_RO`` : float
        ``LOOSE`` : float
        ``FIRM`` : float
        ``FRACT`` : float
        ``COH`` : float
        ``CHEW`` : float
        ``MOLAR`` : float
        ``MOUTH`` : float
        ``CARMEL`` : float
        ``AROMA`` : float
        ``SWEET`` : float
        ``SALTY`` : float
        ``VAN`` : float
        ``SOUR`` : float
        ``BITTER`` : float
        ``SPG`` : float
        ``W`` : float
        ``T`` : float
        ``RATIO`` : float
        ``INSTRON`` : string
        ``L`` : float
        ``A`` : float
        ``B`` : float

    }
    with 
        static member rowSize = 232
        static member fromRaw (raw:byte[]) = {
                ``__CKG`` = BitConverter.ToDouble(raw, 0)
                ``HOLES`` = BitConverter.ToDouble(raw, 8)
                ``PROTUS`` = BitConverter.ToDouble(raw, 16)
                ``CONTOUR`` = BitConverter.ToDouble(raw, 24)
                ``CELLSIZE`` = BitConverter.ToDouble(raw, 32)
                ``CELLUNIF`` = BitConverter.ToDouble(raw, 40)
                ``SURF_RO`` = BitConverter.ToDouble(raw, 48)
                ``LOOSE`` = BitConverter.ToDouble(raw, 56)
                ``FIRM`` = BitConverter.ToDouble(raw, 64)
                ``FRACT`` = BitConverter.ToDouble(raw, 72)
                ``COH`` = BitConverter.ToDouble(raw, 80)
                ``CHEW`` = BitConverter.ToDouble(raw, 88)
                ``MOLAR`` = BitConverter.ToDouble(raw, 96)
                ``MOUTH`` = BitConverter.ToDouble(raw, 104)
                ``CARMEL`` = BitConverter.ToDouble(raw, 112)
                ``AROMA`` = BitConverter.ToDouble(raw, 120)
                ``SWEET`` = BitConverter.ToDouble(raw, 128)
                ``SALTY`` = BitConverter.ToDouble(raw, 136)
                ``VAN`` = BitConverter.ToDouble(raw, 144)
                ``SOUR`` = BitConverter.ToDouble(raw, 152)
                ``BITTER`` = BitConverter.ToDouble(raw, 160)
                ``SPG`` = BitConverter.ToDouble(raw, 168)
                ``W`` = BitConverter.ToDouble(raw, 176)
                ``T`` = BitConverter.ToDouble(raw, 184)
                ``RATIO`` = BitConverter.ToDouble(raw, 192)
                ``INSTRON`` = byte2str raw 224 4
                ``L`` = BitConverter.ToDouble(raw, 200)
                ``A`` = BitConverter.ToDouble(raw, 208)
                ``B`` = BitConverter.ToDouble(raw, 216)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*COOKIE.rowSize .. dataStart+(i+1)*COOKIE.rowSize-1]) // chop page 
                |> Array.map COOKIE.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*COOKIE.rowSize .. dataStart+(i+1)*COOKIE.rowSize-1]) // chop page 
                |> Array.map COOKIE.fromRaw
            | _ -> Array.empty<COOKIE>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\cookie.sas7bdat"
            for pp in p do
                yield! COOKIE.p2a pp
        }    

    type CORPTRN = {
        ``Method`` : string
        ``T1test1`` : float
        ``T2test1`` : float
        ``T3test1`` : float
        ``T1test2`` : float
        ``T2test2`` : float
        ``T3test2`` : float

    }
    with 
        static member rowSize = 56
        static member fromRaw (raw:byte[]) = {
                ``Method`` = byte2str raw 48 8
                ``T1test1`` = BitConverter.ToDouble(raw, 0)
                ``T2test1`` = BitConverter.ToDouble(raw, 8)
                ``T3test1`` = BitConverter.ToDouble(raw, 16)
                ``T1test2`` = BitConverter.ToDouble(raw, 24)
                ``T2test2`` = BitConverter.ToDouble(raw, 32)
                ``T3test2`` = BitConverter.ToDouble(raw, 40)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*CORPTRN.rowSize .. dataStart+(i+1)*CORPTRN.rowSize-1]) // chop page 
                |> Array.map CORPTRN.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*CORPTRN.rowSize .. dataStart+(i+1)*CORPTRN.rowSize-1]) // chop page 
                |> Array.map CORPTRN.fromRaw
            | _ -> Array.empty<CORPTRN>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\corptrn.sas7bdat"
            for pp in p do
                yield! CORPTRN.p2a pp
        }    

    type DERMATOLOGY3 = {
        ``outcome`` : float
        ``history`` : float
        ``age`` : float
        ``clnatt1`` : float
        ``clnatt2`` : float
        ``clnatt3`` : float
        ``clnatt4`` : float
        ``clnatt5`` : float
        ``clnatt6`` : float
        ``clnatt7`` : float
        ``clnatt8`` : float
        ``clnatt9`` : float
        ``clnatt10`` : float
        ``hstatt12`` : float
        ``hstatt13`` : float
        ``hstatt14`` : float
        ``hstatt15`` : float
        ``hstatt16`` : float
        ``hstatt17`` : float
        ``hstatt18`` : float
        ``hstatt19`` : float
        ``hstatt20`` : float
        ``hstatt21`` : float
        ``hstatt22`` : float
        ``hstatt23`` : float
        ``hstatt24`` : float
        ``hstatt25`` : float
        ``hstatt26`` : float
        ``hstatt27`` : float
        ``hstatt28`` : float
        ``hstatt29`` : float
        ``hstatt30`` : float
        ``hstatt31`` : float
        ``hstatt32`` : float
        ``hstatt33`` : float

    }
    with 
        static member rowSize = 280
        static member fromRaw (raw:byte[]) = {
                ``outcome`` = BitConverter.ToDouble(raw, 0)
                ``history`` = BitConverter.ToDouble(raw, 8)
                ``age`` = BitConverter.ToDouble(raw, 16)
                ``clnatt1`` = BitConverter.ToDouble(raw, 24)
                ``clnatt2`` = BitConverter.ToDouble(raw, 32)
                ``clnatt3`` = BitConverter.ToDouble(raw, 40)
                ``clnatt4`` = BitConverter.ToDouble(raw, 48)
                ``clnatt5`` = BitConverter.ToDouble(raw, 56)
                ``clnatt6`` = BitConverter.ToDouble(raw, 64)
                ``clnatt7`` = BitConverter.ToDouble(raw, 72)
                ``clnatt8`` = BitConverter.ToDouble(raw, 80)
                ``clnatt9`` = BitConverter.ToDouble(raw, 88)
                ``clnatt10`` = BitConverter.ToDouble(raw, 96)
                ``hstatt12`` = BitConverter.ToDouble(raw, 104)
                ``hstatt13`` = BitConverter.ToDouble(raw, 112)
                ``hstatt14`` = BitConverter.ToDouble(raw, 120)
                ``hstatt15`` = BitConverter.ToDouble(raw, 128)
                ``hstatt16`` = BitConverter.ToDouble(raw, 136)
                ``hstatt17`` = BitConverter.ToDouble(raw, 144)
                ``hstatt18`` = BitConverter.ToDouble(raw, 152)
                ``hstatt19`` = BitConverter.ToDouble(raw, 160)
                ``hstatt20`` = BitConverter.ToDouble(raw, 168)
                ``hstatt21`` = BitConverter.ToDouble(raw, 176)
                ``hstatt22`` = BitConverter.ToDouble(raw, 184)
                ``hstatt23`` = BitConverter.ToDouble(raw, 192)
                ``hstatt24`` = BitConverter.ToDouble(raw, 200)
                ``hstatt25`` = BitConverter.ToDouble(raw, 208)
                ``hstatt26`` = BitConverter.ToDouble(raw, 216)
                ``hstatt27`` = BitConverter.ToDouble(raw, 224)
                ``hstatt28`` = BitConverter.ToDouble(raw, 232)
                ``hstatt29`` = BitConverter.ToDouble(raw, 240)
                ``hstatt30`` = BitConverter.ToDouble(raw, 248)
                ``hstatt31`` = BitConverter.ToDouble(raw, 256)
                ``hstatt32`` = BitConverter.ToDouble(raw, 264)
                ``hstatt33`` = BitConverter.ToDouble(raw, 272)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*DERMATOLOGY3.rowSize .. dataStart+(i+1)*DERMATOLOGY3.rowSize-1]) // chop page 
                |> Array.map DERMATOLOGY3.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*DERMATOLOGY3.rowSize .. dataStart+(i+1)*DERMATOLOGY3.rowSize-1]) // chop page 
                |> Array.map DERMATOLOGY3.fromRaw
            | _ -> Array.empty<DERMATOLOGY3>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\dermatology.sas7bdat"
            for pp in p do
                yield! DERMATOLOGY3.p2a pp
        }    

    type ELONGATED = {
        ``x`` : float
        ``y`` : float

    }
    with 
        static member rowSize = 16
        static member fromRaw (raw:byte[]) = {
                ``x`` = BitConverter.ToDouble(raw, 0)
                ``y`` = BitConverter.ToDouble(raw, 8)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*ELONGATED.rowSize .. dataStart+(i+1)*ELONGATED.rowSize-1]) // chop page 
                |> Array.map ELONGATED.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*ELONGATED.rowSize .. dataStart+(i+1)*ELONGATED.rowSize-1]) // chop page 
                |> Array.map ELONGATED.fromRaw
            | _ -> Array.empty<ELONGATED>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\elongated.sas7bdat"
            for pp in p do
                yield! ELONGATED.p2a pp
        }    

    type EX8_1 = {
        ``ID`` : float
        ``SEX`` : string
        ``MAJOR`` : string
        ``AGE`` : float
        ``GPT`` : float
        ``HRS`` : float
        ``RISK`` : string

    }
    with 
        static member rowSize = 56
        static member fromRaw (raw:byte[]) = {
                ``ID`` = BitConverter.ToDouble(raw, 0)
                ``SEX`` = byte2str raw 32 8
                ``MAJOR`` = byte2str raw 40 8
                ``AGE`` = BitConverter.ToDouble(raw, 8)
                ``GPT`` = BitConverter.ToDouble(raw, 16)
                ``HRS`` = BitConverter.ToDouble(raw, 24)
                ``RISK`` = byte2str raw 48 4
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*EX8_1.rowSize .. dataStart+(i+1)*EX8_1.rowSize-1]) // chop page 
                |> Array.map EX8_1.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*EX8_1.rowSize .. dataStart+(i+1)*EX8_1.rowSize-1]) // chop page 
                |> Array.map EX8_1.fromRaw
            | _ -> Array.empty<EX8_1>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\ex8_1.sas7bdat"
            for pp in p do
                yield! EX8_1.p2a pp
        }    

    type FIRO = {
        ``FAD01`` : float
        ``FAD02`` : float
        ``FAD03`` : float
        ``FAD04`` : float
        ``FAD05`` : float
        ``FAD06`` : float
        ``FAD07`` : float
        ``FAD08`` : float
        ``FAD09`` : float
        ``FAD10`` : float
        ``FAD11`` : float
        ``FAD12`` : float
        ``FAD13`` : float
        ``FAD14`` : float
        ``FAD15`` : float
        ``FAD16`` : float
        ``FAD17`` : float
        ``FAD18`` : float
        ``FAD19`` : float
        ``FAD20`` : float
        ``FAD21`` : float
        ``FAD22`` : float
        ``FAD23`` : float
        ``FAD24`` : float
        ``FAD25`` : float
        ``FAD26`` : float
        ``FAD27`` : float
        ``FAD28`` : float
        ``FAD29`` : float
        ``FAD30`` : float
        ``FAD31`` : float
        ``FAD32`` : float
        ``FAD33`` : float
        ``FAD34`` : float
        ``FAD35`` : float
        ``FAD36`` : float
        ``FAD37`` : float
        ``FAD38`` : float
        ``FAD39`` : float
        ``FAD40`` : float
        ``FAD41`` : float
        ``FAD42`` : float
        ``FAD43`` : float
        ``FAD44`` : float
        ``FAD45`` : float
        ``FAD46`` : float
        ``FAD47`` : float
        ``FAD48`` : float
        ``FAD49`` : float
        ``FAD50`` : float
        ``FAD51`` : float
        ``FAD52`` : float
        ``FAD53`` : float
        ``FAD54`` : float
        ``FAD55`` : float
        ``FAD56`` : float
        ``FAD57`` : float
        ``FAD58`` : float
        ``FAD59`` : float
        ``FAD60`` : float
        ``KFLS1`` : float
        ``KFLS2`` : float
        ``KFLS3`` : float
        ``KFLS4`` : float
        ``KMS1`` : float
        ``KMS2`` : float
        ``KMS3`` : float
        ``EMC1`` : float
        ``EMC2`` : float
        ``EMC3`` : float
        ``EMC4`` : float
        ``EMC5`` : float
        ``EMC6`` : float
        ``FACES1`` : float
        ``FACES2`` : float
        ``FACES3`` : float
        ``FACES4`` : float
        ``FACES5`` : float
        ``FACES6`` : float
        ``FACES7`` : float
        ``FACES8`` : float
        ``FACES9`` : float
        ``FACES10`` : float
        ``FACES11`` : float
        ``FACES12`` : float
        ``FACES13`` : float
        ``FACES14`` : float
        ``FACES15`` : float
        ``FACES16`` : float
        ``FACES17`` : float
        ``FACES18`` : float
        ``FACES19`` : float
        ``FACES20`` : float
        ``FACES21`` : float
        ``FACES22`` : float
        ``FACES23`` : float
        ``FACES24`` : float
        ``FACES25`` : float
        ``FACES26`` : float
        ``FACES27`` : float
        ``FACES28`` : float
        ``FACES29`` : float
        ``FACES30`` : float
        ``AGE`` : float
        ``SEX`` : float
        ``RACE`` : float
        ``EDU`` : float
        ``INCOME`` : float
        ``MARITAL`` : float
        ``YRSMAR`` : float
        ``CHLD1SEX`` : float
        ``CHLD1AGE`` : float
        ``CHLD1TYP`` : float
        ``CHLD2SEX`` : float
        ``CHLD2AGE`` : float
        ``CHLD2TYP`` : float
        ``CHLD3SEX`` : float
        ``CHLD3AGE`` : float
        ``CHLD3TYP`` : float
        ``CHLD4SEX`` : float
        ``CHLD4AGE`` : float
        ``CHLD4TYP`` : float
        ``CHLD5SEX`` : float
        ``CHLD5AGE`` : float
        ``CHLD5TYP`` : float
        ``CHLD6SEX`` : float
        ``CHLD6AGE`` : float
        ``CHLD6TYP`` : float
        ``CHLD7SEX`` : float
        ``CHLD7AGE`` : float
        ``CHLD7TYP`` : float
        ``CHLD8SEX`` : float
        ``CHLD8AGE`` : float
        ``CHLD8TYP`` : float
        ``COUPLE`` : float
        ``FAMSIZE`` : float

    }
    with 
        static member rowSize = 1088
        static member fromRaw (raw:byte[]) = {
                ``FAD01`` = BitConverter.ToDouble(raw, 0)
                ``FAD02`` = BitConverter.ToDouble(raw, 8)
                ``FAD03`` = BitConverter.ToDouble(raw, 16)
                ``FAD04`` = BitConverter.ToDouble(raw, 24)
                ``FAD05`` = BitConverter.ToDouble(raw, 32)
                ``FAD06`` = BitConverter.ToDouble(raw, 40)
                ``FAD07`` = BitConverter.ToDouble(raw, 48)
                ``FAD08`` = BitConverter.ToDouble(raw, 56)
                ``FAD09`` = BitConverter.ToDouble(raw, 64)
                ``FAD10`` = BitConverter.ToDouble(raw, 72)
                ``FAD11`` = BitConverter.ToDouble(raw, 80)
                ``FAD12`` = BitConverter.ToDouble(raw, 88)
                ``FAD13`` = BitConverter.ToDouble(raw, 96)
                ``FAD14`` = BitConverter.ToDouble(raw, 104)
                ``FAD15`` = BitConverter.ToDouble(raw, 112)
                ``FAD16`` = BitConverter.ToDouble(raw, 120)
                ``FAD17`` = BitConverter.ToDouble(raw, 128)
                ``FAD18`` = BitConverter.ToDouble(raw, 136)
                ``FAD19`` = BitConverter.ToDouble(raw, 144)
                ``FAD20`` = BitConverter.ToDouble(raw, 152)
                ``FAD21`` = BitConverter.ToDouble(raw, 160)
                ``FAD22`` = BitConverter.ToDouble(raw, 168)
                ``FAD23`` = BitConverter.ToDouble(raw, 176)
                ``FAD24`` = BitConverter.ToDouble(raw, 184)
                ``FAD25`` = BitConverter.ToDouble(raw, 192)
                ``FAD26`` = BitConverter.ToDouble(raw, 200)
                ``FAD27`` = BitConverter.ToDouble(raw, 208)
                ``FAD28`` = BitConverter.ToDouble(raw, 216)
                ``FAD29`` = BitConverter.ToDouble(raw, 224)
                ``FAD30`` = BitConverter.ToDouble(raw, 232)
                ``FAD31`` = BitConverter.ToDouble(raw, 240)
                ``FAD32`` = BitConverter.ToDouble(raw, 248)
                ``FAD33`` = BitConverter.ToDouble(raw, 256)
                ``FAD34`` = BitConverter.ToDouble(raw, 264)
                ``FAD35`` = BitConverter.ToDouble(raw, 272)
                ``FAD36`` = BitConverter.ToDouble(raw, 280)
                ``FAD37`` = BitConverter.ToDouble(raw, 288)
                ``FAD38`` = BitConverter.ToDouble(raw, 296)
                ``FAD39`` = BitConverter.ToDouble(raw, 304)
                ``FAD40`` = BitConverter.ToDouble(raw, 312)
                ``FAD41`` = BitConverter.ToDouble(raw, 320)
                ``FAD42`` = BitConverter.ToDouble(raw, 328)
                ``FAD43`` = BitConverter.ToDouble(raw, 336)
                ``FAD44`` = BitConverter.ToDouble(raw, 344)
                ``FAD45`` = BitConverter.ToDouble(raw, 352)
                ``FAD46`` = BitConverter.ToDouble(raw, 360)
                ``FAD47`` = BitConverter.ToDouble(raw, 368)
                ``FAD48`` = BitConverter.ToDouble(raw, 376)
                ``FAD49`` = BitConverter.ToDouble(raw, 384)
                ``FAD50`` = BitConverter.ToDouble(raw, 392)
                ``FAD51`` = BitConverter.ToDouble(raw, 400)
                ``FAD52`` = BitConverter.ToDouble(raw, 408)
                ``FAD53`` = BitConverter.ToDouble(raw, 416)
                ``FAD54`` = BitConverter.ToDouble(raw, 424)
                ``FAD55`` = BitConverter.ToDouble(raw, 432)
                ``FAD56`` = BitConverter.ToDouble(raw, 440)
                ``FAD57`` = BitConverter.ToDouble(raw, 448)
                ``FAD58`` = BitConverter.ToDouble(raw, 456)
                ``FAD59`` = BitConverter.ToDouble(raw, 464)
                ``FAD60`` = BitConverter.ToDouble(raw, 472)
                ``KFLS1`` = BitConverter.ToDouble(raw, 480)
                ``KFLS2`` = BitConverter.ToDouble(raw, 488)
                ``KFLS3`` = BitConverter.ToDouble(raw, 496)
                ``KFLS4`` = BitConverter.ToDouble(raw, 504)
                ``KMS1`` = BitConverter.ToDouble(raw, 512)
                ``KMS2`` = BitConverter.ToDouble(raw, 520)
                ``KMS3`` = BitConverter.ToDouble(raw, 528)
                ``EMC1`` = BitConverter.ToDouble(raw, 536)
                ``EMC2`` = BitConverter.ToDouble(raw, 544)
                ``EMC3`` = BitConverter.ToDouble(raw, 552)
                ``EMC4`` = BitConverter.ToDouble(raw, 560)
                ``EMC5`` = BitConverter.ToDouble(raw, 568)
                ``EMC6`` = BitConverter.ToDouble(raw, 576)
                ``FACES1`` = BitConverter.ToDouble(raw, 584)
                ``FACES2`` = BitConverter.ToDouble(raw, 592)
                ``FACES3`` = BitConverter.ToDouble(raw, 600)
                ``FACES4`` = BitConverter.ToDouble(raw, 608)
                ``FACES5`` = BitConverter.ToDouble(raw, 616)
                ``FACES6`` = BitConverter.ToDouble(raw, 624)
                ``FACES7`` = BitConverter.ToDouble(raw, 632)
                ``FACES8`` = BitConverter.ToDouble(raw, 640)
                ``FACES9`` = BitConverter.ToDouble(raw, 648)
                ``FACES10`` = BitConverter.ToDouble(raw, 656)
                ``FACES11`` = BitConverter.ToDouble(raw, 664)
                ``FACES12`` = BitConverter.ToDouble(raw, 672)
                ``FACES13`` = BitConverter.ToDouble(raw, 680)
                ``FACES14`` = BitConverter.ToDouble(raw, 688)
                ``FACES15`` = BitConverter.ToDouble(raw, 696)
                ``FACES16`` = BitConverter.ToDouble(raw, 704)
                ``FACES17`` = BitConverter.ToDouble(raw, 712)
                ``FACES18`` = BitConverter.ToDouble(raw, 720)
                ``FACES19`` = BitConverter.ToDouble(raw, 728)
                ``FACES20`` = BitConverter.ToDouble(raw, 736)
                ``FACES21`` = BitConverter.ToDouble(raw, 744)
                ``FACES22`` = BitConverter.ToDouble(raw, 752)
                ``FACES23`` = BitConverter.ToDouble(raw, 760)
                ``FACES24`` = BitConverter.ToDouble(raw, 768)
                ``FACES25`` = BitConverter.ToDouble(raw, 776)
                ``FACES26`` = BitConverter.ToDouble(raw, 784)
                ``FACES27`` = BitConverter.ToDouble(raw, 792)
                ``FACES28`` = BitConverter.ToDouble(raw, 800)
                ``FACES29`` = BitConverter.ToDouble(raw, 808)
                ``FACES30`` = BitConverter.ToDouble(raw, 816)
                ``AGE`` = BitConverter.ToDouble(raw, 824)
                ``SEX`` = BitConverter.ToDouble(raw, 832)
                ``RACE`` = BitConverter.ToDouble(raw, 840)
                ``EDU`` = BitConverter.ToDouble(raw, 848)
                ``INCOME`` = BitConverter.ToDouble(raw, 856)
                ``MARITAL`` = BitConverter.ToDouble(raw, 864)
                ``YRSMAR`` = BitConverter.ToDouble(raw, 872)
                ``CHLD1SEX`` = BitConverter.ToDouble(raw, 880)
                ``CHLD1AGE`` = BitConverter.ToDouble(raw, 888)
                ``CHLD1TYP`` = BitConverter.ToDouble(raw, 896)
                ``CHLD2SEX`` = BitConverter.ToDouble(raw, 904)
                ``CHLD2AGE`` = BitConverter.ToDouble(raw, 912)
                ``CHLD2TYP`` = BitConverter.ToDouble(raw, 920)
                ``CHLD3SEX`` = BitConverter.ToDouble(raw, 928)
                ``CHLD3AGE`` = BitConverter.ToDouble(raw, 936)
                ``CHLD3TYP`` = BitConverter.ToDouble(raw, 944)
                ``CHLD4SEX`` = BitConverter.ToDouble(raw, 952)
                ``CHLD4AGE`` = BitConverter.ToDouble(raw, 960)
                ``CHLD4TYP`` = BitConverter.ToDouble(raw, 968)
                ``CHLD5SEX`` = BitConverter.ToDouble(raw, 976)
                ``CHLD5AGE`` = BitConverter.ToDouble(raw, 984)
                ``CHLD5TYP`` = BitConverter.ToDouble(raw, 992)
                ``CHLD6SEX`` = BitConverter.ToDouble(raw, 1000)
                ``CHLD6AGE`` = BitConverter.ToDouble(raw, 1008)
                ``CHLD6TYP`` = BitConverter.ToDouble(raw, 1016)
                ``CHLD7SEX`` = BitConverter.ToDouble(raw, 1024)
                ``CHLD7AGE`` = BitConverter.ToDouble(raw, 1032)
                ``CHLD7TYP`` = BitConverter.ToDouble(raw, 1040)
                ``CHLD8SEX`` = BitConverter.ToDouble(raw, 1048)
                ``CHLD8AGE`` = BitConverter.ToDouble(raw, 1056)
                ``CHLD8TYP`` = BitConverter.ToDouble(raw, 1064)
                ``COUPLE`` = BitConverter.ToDouble(raw, 1072)
                ``FAMSIZE`` = BitConverter.ToDouble(raw, 1080)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*FIRO.rowSize .. dataStart+(i+1)*FIRO.rowSize-1]) // chop page 
                |> Array.map FIRO.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*FIRO.rowSize .. dataStart+(i+1)*FIRO.rowSize-1]) // chop page 
                |> Array.map FIRO.fromRaw
            | _ -> Array.empty<FIRO>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\firo.sas7bdat"
            for pp in p do
                yield! FIRO.p2a pp
        }    

    type FISH = {
        ``species`` : float
        ``weight`` : float
        ``length1`` : float
        ``length2`` : float
        ``length3`` : float
        ``weight3`` : float
        ``height`` : float
        ``width`` : float
        ``newLength1`` : float
        ``newLength3`` : float
        ``logLengthRatio`` : float
        ``symbol`` : string

    }
    with 
        static member rowSize = 96
        static member fromRaw (raw:byte[]) = {
                ``species`` = BitConverter.ToDouble(raw, 0)
                ``weight`` = BitConverter.ToDouble(raw, 8)
                ``length1`` = BitConverter.ToDouble(raw, 16)
                ``length2`` = BitConverter.ToDouble(raw, 24)
                ``length3`` = BitConverter.ToDouble(raw, 32)
                ``weight3`` = BitConverter.ToDouble(raw, 40)
                ``height`` = BitConverter.ToDouble(raw, 48)
                ``width`` = BitConverter.ToDouble(raw, 56)
                ``newLength1`` = BitConverter.ToDouble(raw, 64)
                ``newLength3`` = BitConverter.ToDouble(raw, 72)
                ``logLengthRatio`` = BitConverter.ToDouble(raw, 80)
                ``symbol`` = byte2str raw 88 2
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*FISH.rowSize .. dataStart+(i+1)*FISH.rowSize-1]) // chop page 
                |> Array.map FISH.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*FISH.rowSize .. dataStart+(i+1)*FISH.rowSize-1]) // chop page 
                |> Array.map FISH.fromRaw
            | _ -> Array.empty<FISH>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\fish.sas7bdat"
            for pp in p do
                yield! FISH.p2a pp
        }    

    type GAMBLEGRP = {
        ``sub`` : float
        ``dsm1`` : float
        ``dsm2`` : float
        ``dsm3`` : float
        ``dsm4`` : float
        ``dsm5`` : float
        ``dsm6`` : float
        ``dsm7`` : float
        ``dsm8`` : float
        ``dsm9`` : float
        ``dsm10`` : float
        ``dsm11`` : float
        ``dsm12`` : float
        ``ga1`` : float
        ``ga2`` : float
        ``ga3`` : float
        ``ga4`` : float
        ``ga5`` : float
        ``ga6`` : float
        ``ga7`` : float
        ``ga8`` : float
        ``ga9`` : float
        ``ga10`` : float
        ``ga11`` : float
        ``ga12`` : float
        ``ga13`` : float
        ``ga14`` : float
        ``ga15`` : float
        ``ga16`` : float
        ``ga17`` : float
        ``ga18`` : float
        ``ga19`` : float
        ``ga20`` : float
        ``type`` : string

    }
    with 
        static member rowSize = 280
        static member fromRaw (raw:byte[]) = {
                ``sub`` = BitConverter.ToDouble(raw, 0)
                ``dsm1`` = BitConverter.ToDouble(raw, 8)
                ``dsm2`` = BitConverter.ToDouble(raw, 16)
                ``dsm3`` = BitConverter.ToDouble(raw, 24)
                ``dsm4`` = BitConverter.ToDouble(raw, 32)
                ``dsm5`` = BitConverter.ToDouble(raw, 40)
                ``dsm6`` = BitConverter.ToDouble(raw, 48)
                ``dsm7`` = BitConverter.ToDouble(raw, 56)
                ``dsm8`` = BitConverter.ToDouble(raw, 64)
                ``dsm9`` = BitConverter.ToDouble(raw, 72)
                ``dsm10`` = BitConverter.ToDouble(raw, 80)
                ``dsm11`` = BitConverter.ToDouble(raw, 88)
                ``dsm12`` = BitConverter.ToDouble(raw, 96)
                ``ga1`` = BitConverter.ToDouble(raw, 104)
                ``ga2`` = BitConverter.ToDouble(raw, 112)
                ``ga3`` = BitConverter.ToDouble(raw, 120)
                ``ga4`` = BitConverter.ToDouble(raw, 128)
                ``ga5`` = BitConverter.ToDouble(raw, 136)
                ``ga6`` = BitConverter.ToDouble(raw, 144)
                ``ga7`` = BitConverter.ToDouble(raw, 152)
                ``ga8`` = BitConverter.ToDouble(raw, 160)
                ``ga9`` = BitConverter.ToDouble(raw, 168)
                ``ga10`` = BitConverter.ToDouble(raw, 176)
                ``ga11`` = BitConverter.ToDouble(raw, 184)
                ``ga12`` = BitConverter.ToDouble(raw, 192)
                ``ga13`` = BitConverter.ToDouble(raw, 200)
                ``ga14`` = BitConverter.ToDouble(raw, 208)
                ``ga15`` = BitConverter.ToDouble(raw, 216)
                ``ga16`` = BitConverter.ToDouble(raw, 224)
                ``ga17`` = BitConverter.ToDouble(raw, 232)
                ``ga18`` = BitConverter.ToDouble(raw, 240)
                ``ga19`` = BitConverter.ToDouble(raw, 248)
                ``ga20`` = BitConverter.ToDouble(raw, 256)
                ``type`` = byte2str raw 264 11
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*GAMBLEGRP.rowSize .. dataStart+(i+1)*GAMBLEGRP.rowSize-1]) // chop page 
                |> Array.map GAMBLEGRP.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*GAMBLEGRP.rowSize .. dataStart+(i+1)*GAMBLEGRP.rowSize-1]) // chop page 
                |> Array.map GAMBLEGRP.fromRaw
            | _ -> Array.empty<GAMBLEGRP>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\gamblegrp.sas7bdat"
            for pp in p do
                yield! GAMBLEGRP.p2a pp
        }    

    type HOTEL = {
        ``LOCATION`` : string
        ``ROOMS`` : float
        ``MIN`` : float
        ``MAX`` : float
        ``DINING`` : string
        ``LOUNGE`` : string
        ``BKFST`` : string
        ``POOL`` : string
        ``TYPE`` : string

    }
    with 
        static member rowSize = 88
        static member fromRaw (raw:byte[]) = {
                ``LOCATION`` = byte2str raw 24 18
                ``ROOMS`` = BitConverter.ToDouble(raw, 0)
                ``MIN`` = BitConverter.ToDouble(raw, 8)
                ``MAX`` = BitConverter.ToDouble(raw, 16)
                ``DINING`` = byte2str raw 42 8
                ``LOUNGE`` = byte2str raw 50 8
                ``BKFST`` = byte2str raw 58 8
                ``POOL`` = byte2str raw 66 8
                ``TYPE`` = byte2str raw 74 8
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*HOTEL.rowSize .. dataStart+(i+1)*HOTEL.rowSize-1]) // chop page 
                |> Array.map HOTEL.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*HOTEL.rowSize .. dataStart+(i+1)*HOTEL.rowSize-1]) // chop page 
                |> Array.map HOTEL.fromRaw
            | _ -> Array.empty<HOTEL>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\hotel.sas7bdat"
            for pp in p do
                yield! HOTEL.p2a pp
        }    

    type IRIS = {
        ``variety`` : string
        ``sl`` : float
        ``sw`` : float
        ``pl`` : float
        ``pw`` : float

    }
    with 
        static member rowSize = 40
        static member fromRaw (raw:byte[]) = {
                ``variety`` = byte2str raw 32 8
                ``sl`` = BitConverter.ToDouble(raw, 0)
                ``sw`` = BitConverter.ToDouble(raw, 8)
                ``pl`` = BitConverter.ToDouble(raw, 16)
                ``pw`` = BitConverter.ToDouble(raw, 24)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*IRIS.rowSize .. dataStart+(i+1)*IRIS.rowSize-1]) // chop page 
                |> Array.map IRIS.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*IRIS.rowSize .. dataStart+(i+1)*IRIS.rowSize-1]) // chop page 
                |> Array.map IRIS.fromRaw
            | _ -> Array.empty<IRIS>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\iris.sas7bdat"
            for pp in p do
                yield! IRIS.p2a pp
        }    

    type MAMMALS = {
        ``beaver`` : float
        ``camel`` : float
        ``wolf`` : float
        ``donkey`` : float
        ``gorilla`` : float
        ``tiger`` : float
        ``bear`` : float
        ``fox`` : float
        ``antelope`` : float
        ``deer`` : float
        ``giraffe`` : float
        ``chipmunk`` : float
        ``raccoon`` : float
        ``rabbit`` : float
        ``monkey`` : float
        ``rat`` : float
        ``elephant`` : float
        ``goat`` : float
        ``squirrel`` : float
        ``sheep`` : float
        ``chimpanze`` : float
        ``lion`` : float
        ``zebra`` : float
        ``horse`` : float
        ``dog`` : float
        ``leopard`` : float
        ``pig`` : float
        ``cow`` : float
        ``cat`` : float
        ``mouse`` : float
        ``name`` : string

    }
    with 
        static member rowSize = 256
        static member fromRaw (raw:byte[]) = {
                ``beaver`` = BitConverter.ToDouble(raw, 0)
                ``camel`` = BitConverter.ToDouble(raw, 8)
                ``wolf`` = BitConverter.ToDouble(raw, 16)
                ``donkey`` = BitConverter.ToDouble(raw, 24)
                ``gorilla`` = BitConverter.ToDouble(raw, 32)
                ``tiger`` = BitConverter.ToDouble(raw, 40)
                ``bear`` = BitConverter.ToDouble(raw, 48)
                ``fox`` = BitConverter.ToDouble(raw, 56)
                ``antelope`` = BitConverter.ToDouble(raw, 64)
                ``deer`` = BitConverter.ToDouble(raw, 72)
                ``giraffe`` = BitConverter.ToDouble(raw, 80)
                ``chipmunk`` = BitConverter.ToDouble(raw, 88)
                ``raccoon`` = BitConverter.ToDouble(raw, 96)
                ``rabbit`` = BitConverter.ToDouble(raw, 104)
                ``monkey`` = BitConverter.ToDouble(raw, 112)
                ``rat`` = BitConverter.ToDouble(raw, 120)
                ``elephant`` = BitConverter.ToDouble(raw, 128)
                ``goat`` = BitConverter.ToDouble(raw, 136)
                ``squirrel`` = BitConverter.ToDouble(raw, 144)
                ``sheep`` = BitConverter.ToDouble(raw, 152)
                ``chimpanze`` = BitConverter.ToDouble(raw, 160)
                ``lion`` = BitConverter.ToDouble(raw, 168)
                ``zebra`` = BitConverter.ToDouble(raw, 176)
                ``horse`` = BitConverter.ToDouble(raw, 184)
                ``dog`` = BitConverter.ToDouble(raw, 192)
                ``leopard`` = BitConverter.ToDouble(raw, 200)
                ``pig`` = BitConverter.ToDouble(raw, 208)
                ``cow`` = BitConverter.ToDouble(raw, 216)
                ``cat`` = BitConverter.ToDouble(raw, 224)
                ``mouse`` = BitConverter.ToDouble(raw, 232)
                ``name`` = byte2str raw 240 15
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*MAMMALS.rowSize .. dataStart+(i+1)*MAMMALS.rowSize-1]) // chop page 
                |> Array.map MAMMALS.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*MAMMALS.rowSize .. dataStart+(i+1)*MAMMALS.rowSize-1]) // chop page 
                |> Array.map MAMMALS.fromRaw
            | _ -> Array.empty<MAMMALS>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\mammals.sas7bdat"
            for pp in p do
                yield! MAMMALS.p2a pp
        }    

    type MATHATTITUDES = {
        ``CLASS`` : float
        ``STUDENT`` : float
        ``XAGE`` : float
        ``C2`` : float
        ``C5`` : float
        ``C7`` : float
        ``C13`` : float
        ``C18`` : float
        ``C21`` : float
        ``C38`` : float
        ``C39`` : float
        ``C42`` : float
        ``C44`` : float
        ``C46`` : float
        ``C51`` : float

    }
    with 
        static member rowSize = 120
        static member fromRaw (raw:byte[]) = {
                ``CLASS`` = BitConverter.ToDouble(raw, 0)
                ``STUDENT`` = BitConverter.ToDouble(raw, 8)
                ``XAGE`` = BitConverter.ToDouble(raw, 16)
                ``C2`` = BitConverter.ToDouble(raw, 24)
                ``C5`` = BitConverter.ToDouble(raw, 32)
                ``C7`` = BitConverter.ToDouble(raw, 40)
                ``C13`` = BitConverter.ToDouble(raw, 48)
                ``C18`` = BitConverter.ToDouble(raw, 56)
                ``C21`` = BitConverter.ToDouble(raw, 64)
                ``C38`` = BitConverter.ToDouble(raw, 72)
                ``C39`` = BitConverter.ToDouble(raw, 80)
                ``C42`` = BitConverter.ToDouble(raw, 88)
                ``C44`` = BitConverter.ToDouble(raw, 96)
                ``C46`` = BitConverter.ToDouble(raw, 104)
                ``C51`` = BitConverter.ToDouble(raw, 112)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*MATHATTITUDES.rowSize .. dataStart+(i+1)*MATHATTITUDES.rowSize-1]) // chop page 
                |> Array.map MATHATTITUDES.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*MATHATTITUDES.rowSize .. dataStart+(i+1)*MATHATTITUDES.rowSize-1]) // chop page 
                |> Array.map MATHATTITUDES.fromRaw
            | _ -> Array.empty<MATHATTITUDES>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\mathattitudes.sas7bdat"
            for pp in p do
                yield! MATHATTITUDES.p2a pp
        }    

    type NCAA = {
        ``CODE`` : float
        ``NAME`` : string
        ``ALLSTUDT`` : string
        ``ALLATHL`` : string
        ``BASEBALL`` : string
        ``MENBASK`` : string
        ``WOMBASK`` : string
        ``FOOTBALL`` : string

    }
    with 
        static member rowSize = 48
        static member fromRaw (raw:byte[]) = {
                ``CODE`` = BitConverter.ToDouble(raw, 0)
                ``NAME`` = byte2str raw 8 19
                ``ALLSTUDT`` = byte2str raw 27 2
                ``ALLATHL`` = byte2str raw 29 2
                ``BASEBALL`` = byte2str raw 31 3
                ``MENBASK`` = byte2str raw 34 3
                ``WOMBASK`` = byte2str raw 37 3
                ``FOOTBALL`` = byte2str raw 40 2
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*NCAA.rowSize .. dataStart+(i+1)*NCAA.rowSize-1]) // chop page 
                |> Array.map NCAA.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*NCAA.rowSize .. dataStart+(i+1)*NCAA.rowSize-1]) // chop page 
                |> Array.map NCAA.fromRaw
            | _ -> Array.empty<NCAA>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\ncaa.sas7bdat"
            for pp in p do
                yield! NCAA.p2a pp
        }    

    type OCD = {
        ``subject`` : float
        ``drug`` : string
        ``dose`` : float
        ``hdrsgain`` : float
        ``ybocgain`` : float
        ``nimhgain`` : float

    }
    with 
        static member rowSize = 48
        static member fromRaw (raw:byte[]) = {
                ``subject`` = BitConverter.ToDouble(raw, 0)
                ``drug`` = byte2str raw 40 5
                ``dose`` = BitConverter.ToDouble(raw, 8)
                ``hdrsgain`` = BitConverter.ToDouble(raw, 16)
                ``ybocgain`` = BitConverter.ToDouble(raw, 24)
                ``nimhgain`` = BitConverter.ToDouble(raw, 32)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*OCD.rowSize .. dataStart+(i+1)*OCD.rowSize-1]) // chop page 
                |> Array.map OCD.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*OCD.rowSize .. dataStart+(i+1)*OCD.rowSize-1]) // chop page 
                |> Array.map OCD.fromRaw
            | _ -> Array.empty<OCD>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\ocd.sas7bdat"
            for pp in p do
                yield! OCD.p2a pp
        }    

    type ORANGE = {
        ``__FDA_ID`` : float
        ``COUNTRY`` : string
        ``B`` : float
        ``BA`` : float
        ``CA`` : float
        ``K`` : float
        ``MG`` : float
        ``MN`` : float
        ``P`` : float
        ``RB`` : float
        ``ZN`` : float

    }
    with 
        static member rowSize = 88
        static member fromRaw (raw:byte[]) = {
                ``__FDA_ID`` = BitConverter.ToDouble(raw, 0)
                ``COUNTRY`` = byte2str raw 80 3
                ``B`` = BitConverter.ToDouble(raw, 8)
                ``BA`` = BitConverter.ToDouble(raw, 16)
                ``CA`` = BitConverter.ToDouble(raw, 24)
                ``K`` = BitConverter.ToDouble(raw, 32)
                ``MG`` = BitConverter.ToDouble(raw, 40)
                ``MN`` = BitConverter.ToDouble(raw, 48)
                ``P`` = BitConverter.ToDouble(raw, 56)
                ``RB`` = BitConverter.ToDouble(raw, 64)
                ``ZN`` = BitConverter.ToDouble(raw, 72)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*ORANGE.rowSize .. dataStart+(i+1)*ORANGE.rowSize-1]) // chop page 
                |> Array.map ORANGE.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*ORANGE.rowSize .. dataStart+(i+1)*ORANGE.rowSize-1]) // chop page 
                |> Array.map ORANGE.fromRaw
            | _ -> Array.empty<ORANGE>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\orange.sas7bdat"
            for pp in p do
                yield! ORANGE.p2a pp
        }    

    type PEN = {
        ``X1`` : float
        ``X10`` : float
        ``DIGIT`` : float

    }
    with 
        static member rowSize = 24
        static member fromRaw (raw:byte[]) = {
                ``X1`` = BitConverter.ToDouble(raw, 0)
                ``X10`` = BitConverter.ToDouble(raw, 8)
                ``DIGIT`` = BitConverter.ToDouble(raw, 16)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*PEN.rowSize .. dataStart+(i+1)*PEN.rowSize-1]) // chop page 
                |> Array.map PEN.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*PEN.rowSize .. dataStart+(i+1)*PEN.rowSize-1]) // chop page 
                |> Array.map PEN.fromRaw
            | _ -> Array.empty<PEN>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\pen.sas7bdat"
            for pp in p do
                yield! PEN.p2a pp
        }    

    type PERSONAE = {
        ``SAVINGS`` : float
        ``CHECK`` : float
        ``CD`` : float
        ``MMKT`` : float
        ``MORTGAGE`` : float
        ``TRIPS`` : float
        ``CATALOGS`` : float
        ``LATEPAY`` : float
        ``REVOLVE`` : float
        ``VACCOST`` : float
        ``INCOME`` : float
        ``JOB2`` : float
        ``LOCLOAN`` : float
        ``EATOUT`` : float
        ``HOTELS`` : float
        ``CLUBS`` : float
        ``GROUP`` : string

    }
    with 
        static member rowSize = 136
        static member fromRaw (raw:byte[]) = {
                ``SAVINGS`` = BitConverter.ToDouble(raw, 0)
                ``CHECK`` = BitConverter.ToDouble(raw, 8)
                ``CD`` = BitConverter.ToDouble(raw, 16)
                ``MMKT`` = BitConverter.ToDouble(raw, 24)
                ``MORTGAGE`` = BitConverter.ToDouble(raw, 32)
                ``TRIPS`` = BitConverter.ToDouble(raw, 40)
                ``CATALOGS`` = BitConverter.ToDouble(raw, 48)
                ``LATEPAY`` = BitConverter.ToDouble(raw, 56)
                ``REVOLVE`` = BitConverter.ToDouble(raw, 64)
                ``VACCOST`` = BitConverter.ToDouble(raw, 72)
                ``INCOME`` = BitConverter.ToDouble(raw, 80)
                ``JOB2`` = BitConverter.ToDouble(raw, 88)
                ``LOCLOAN`` = BitConverter.ToDouble(raw, 96)
                ``EATOUT`` = BitConverter.ToDouble(raw, 104)
                ``HOTELS`` = BitConverter.ToDouble(raw, 112)
                ``CLUBS`` = BitConverter.ToDouble(raw, 120)
                ``GROUP`` = byte2str raw 128 8
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*PERSONAE.rowSize .. dataStart+(i+1)*PERSONAE.rowSize-1]) // chop page 
                |> Array.map PERSONAE.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*PERSONAE.rowSize .. dataStart+(i+1)*PERSONAE.rowSize-1]) // chop page 
                |> Array.map PERSONAE.fromRaw
            | _ -> Array.empty<PERSONAE>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\personae.sas7bdat"
            for pp in p do
                yield! PERSONAE.p2a pp
        }    

    type PERSONS = {
        ``ID`` : float
        ``VNM`` : string
        ``TSV`` : string
        ``ANM`` : string

    }
    with 
        static member rowSize = 32
        static member fromRaw (raw:byte[]) = {
                ``ID`` = BitConverter.ToDouble(raw, 0)
                ``VNM`` = byte2str raw 8 8
                ``TSV`` = byte2str raw 16 8
                ``ANM`` = byte2str raw 24 8
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*PERSONS.rowSize .. dataStart+(i+1)*PERSONS.rowSize-1]) // chop page 
                |> Array.map PERSONS.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*PERSONS.rowSize .. dataStart+(i+1)*PERSONS.rowSize-1]) // chop page 
                |> Array.map PERSONS.fromRaw
            | _ -> Array.empty<PERSONS>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\persons.sas7bdat"
            for pp in p do
                yield! PERSONS.p2a pp
        }    

    type PIZZA = {
        ``id`` : string
        ``mois`` : float
        ``prot`` : float
        ``fat`` : float
        ``ash`` : float
        ``sodium`` : float
        ``carb`` : float
        ``cal`` : float
        ``brand`` : string

    }
    with 
        static member rowSize = 64
        static member fromRaw (raw:byte[]) = {
                ``id`` = byte2str raw 56 5
                ``mois`` = BitConverter.ToDouble(raw, 0)
                ``prot`` = BitConverter.ToDouble(raw, 8)
                ``fat`` = BitConverter.ToDouble(raw, 16)
                ``ash`` = BitConverter.ToDouble(raw, 24)
                ``sodium`` = BitConverter.ToDouble(raw, 32)
                ``carb`` = BitConverter.ToDouble(raw, 40)
                ``cal`` = BitConverter.ToDouble(raw, 48)
                ``brand`` = byte2str raw 61 1
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*PIZZA.rowSize .. dataStart+(i+1)*PIZZA.rowSize-1]) // chop page 
                |> Array.map PIZZA.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*PIZZA.rowSize .. dataStart+(i+1)*PIZZA.rowSize-1]) // chop page 
                |> Array.map PIZZA.fromRaw
            | _ -> Array.empty<PIZZA>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\pizza.sas7bdat"
            for pp in p do
                yield! PIZZA.p2a pp
        }    

    type PIZZAZZ = {
        ``COMMIT1`` : float
        ``COMMIT2`` : float
        ``COMMIT3`` : float
        ``COMMIT4`` : float
        ``COMMIT5`` : float
        ``COMMIT6`` : float
        ``COMMIT7`` : float
        ``COMMIT8`` : float
        ``COMMIT9`` : float
        ``COMMIT10`` : float
        ``COMMIT11`` : float
        ``COMMIT12`` : float
        ``COMMIT13`` : float
        ``COMMIT14`` : float
        ``COMMIT15`` : float
        ``JOBINV1`` : float
        ``JOBINV2`` : float
        ``JOBINV3`` : float
        ``SERVA1`` : float
        ``SERVA2`` : float
        ``SERVE1`` : float
        ``SERVE2`` : float
        ``SERVE3`` : float
        ``SERVE4`` : float
        ``SERVE5`` : float
        ``SERVE6`` : float
        ``SERVE7`` : float
        ``INST1`` : float
        ``INST2`` : float
        ``INST3`` : float
        ``INST4`` : float
        ``INST5`` : float
        ``PHI`` : float
        ``JOBSEC1`` : float
        ``JOBSEC2`` : float
        ``JOBSEC3`` : float
        ``JOBSEC4`` : float
        ``JOBSEC5`` : float
        ``JOBSEC6`` : float
        ``JOBSEC7`` : float
        ``JOBSEC8`` : float
        ``JOBSEC9`` : float
        ``JOBSEC10`` : float
        ``JOBSEC11`` : float
        ``JOBSEC12`` : float
        ``JOBSEC13`` : float
        ``JOBSEC14`` : float
        ``JOBSEC15`` : float
        ``JOBSEC16`` : float
        ``JOBSEC17`` : float
        ``JOBSEC18`` : float
        ``JOBSEC19`` : float
        ``JOBSEC20`` : float
        ``TQUIT`` : float
        ``ABSENT1`` : float
        ``ABSENT2`` : float
        ``PHIA1`` : float
        ``PHIA2`` : float
        ``PHIA3`` : float
        ``PHIA4`` : float
        ``JOBSAT1`` : float
        ``JOBSAT2`` : float
        ``JOBSAT3`` : float
        ``JOBSAT4`` : float
        ``JOBSAT5`` : float
        ``JOBSAT6`` : float
        ``JOBSAT7`` : float
        ``JOBSAT8`` : float
        ``JOBSAT9`` : float
        ``JOBSAT10`` : float
        ``JOBSAT11`` : float
        ``JOBSAT12`` : float
        ``JOBSAT13`` : float
        ``JOBSAT14`` : float
        ``JOBSAT15`` : float
        ``JOBSAT16`` : float
        ``JOBSAT17`` : float
        ``JOBSAT18`` : float
        ``JOBSAT19`` : float
        ``JOBSAT20`` : float
        ``PHIB1`` : float
        ``PHIB2`` : float
        ``QUIT1`` : float
        ``QUIT2`` : float
        ``QUIT3`` : float
        ``ABSENCES`` : string
        ``PHIC1`` : float
        ``PHIC2`` : float
        ``PHIC3`` : float
        ``PHIC4`` : float
        ``PHIC5`` : float
        ``PHIC6`` : float
        ``PHIC7`` : float
        ``PHIC8`` : float
        ``PHIC9`` : float
        ``PHIC10`` : float
        ``PHIC11`` : float
        ``PHIC12`` : float
        ``PHIC13`` : float
        ``PHIC14`` : float
        ``TEAM1`` : float
        ``TEAM2`` : float
        ``TEAM3`` : float
        ``PHID1`` : float
        ``PHID2`` : float
        ``PHID3`` : float
        ``PHID4`` : float
        ``PHID5`` : float
        ``PHID6`` : float
        ``PHID7`` : float
        ``PHIE1`` : float
        ``PHIE2`` : float
        ``PHIE3`` : float
        ``PHIE4`` : float
        ``PHIE5`` : float
        ``PHIE6`` : float
        ``PHIE7`` : float
        ``PHIE8`` : float
        ``PHIE9`` : float
        ``PHIE10`` : float
        ``PHIE11`` : float
        ``PHIE12`` : float
        ``DIVISION`` : float
        ``AGE`` : float
        ``SEX`` : float
        ``RACE`` : float
        ``MSTATUS`` : float
        ``NCHILD`` : float
        ``EDUC`` : float
        ``POSITION`` : float
        ``SATSCHED`` : float
        ``TYPESCH`` : float
        ``FLEX`` : float
        ``URBRUR`` : float
        ``HOURS`` : float
        ``SCHOOL`` : float
        ``ANJOB`` : float
        ``TYPERES`` : float
        ``LENGTHE`` : float
        ``HOWLONG`` : float
        ``REASONQ`` : float
        ``REASONW`` : float
        ``WHY`` : float
        ``NHOURS`` : float
        ``EMP_ID`` : float

    }
    with 
        static member rowSize = 1160
        static member fromRaw (raw:byte[]) = {
                ``COMMIT1`` = BitConverter.ToDouble(raw, 0)
                ``COMMIT2`` = BitConverter.ToDouble(raw, 8)
                ``COMMIT3`` = BitConverter.ToDouble(raw, 16)
                ``COMMIT4`` = BitConverter.ToDouble(raw, 24)
                ``COMMIT5`` = BitConverter.ToDouble(raw, 32)
                ``COMMIT6`` = BitConverter.ToDouble(raw, 40)
                ``COMMIT7`` = BitConverter.ToDouble(raw, 48)
                ``COMMIT8`` = BitConverter.ToDouble(raw, 56)
                ``COMMIT9`` = BitConverter.ToDouble(raw, 64)
                ``COMMIT10`` = BitConverter.ToDouble(raw, 72)
                ``COMMIT11`` = BitConverter.ToDouble(raw, 80)
                ``COMMIT12`` = BitConverter.ToDouble(raw, 88)
                ``COMMIT13`` = BitConverter.ToDouble(raw, 96)
                ``COMMIT14`` = BitConverter.ToDouble(raw, 104)
                ``COMMIT15`` = BitConverter.ToDouble(raw, 112)
                ``JOBINV1`` = BitConverter.ToDouble(raw, 120)
                ``JOBINV2`` = BitConverter.ToDouble(raw, 128)
                ``JOBINV3`` = BitConverter.ToDouble(raw, 136)
                ``SERVA1`` = BitConverter.ToDouble(raw, 144)
                ``SERVA2`` = BitConverter.ToDouble(raw, 152)
                ``SERVE1`` = BitConverter.ToDouble(raw, 160)
                ``SERVE2`` = BitConverter.ToDouble(raw, 168)
                ``SERVE3`` = BitConverter.ToDouble(raw, 176)
                ``SERVE4`` = BitConverter.ToDouble(raw, 184)
                ``SERVE5`` = BitConverter.ToDouble(raw, 192)
                ``SERVE6`` = BitConverter.ToDouble(raw, 200)
                ``SERVE7`` = BitConverter.ToDouble(raw, 208)
                ``INST1`` = BitConverter.ToDouble(raw, 216)
                ``INST2`` = BitConverter.ToDouble(raw, 224)
                ``INST3`` = BitConverter.ToDouble(raw, 232)
                ``INST4`` = BitConverter.ToDouble(raw, 240)
                ``INST5`` = BitConverter.ToDouble(raw, 248)
                ``PHI`` = BitConverter.ToDouble(raw, 256)
                ``JOBSEC1`` = BitConverter.ToDouble(raw, 264)
                ``JOBSEC2`` = BitConverter.ToDouble(raw, 272)
                ``JOBSEC3`` = BitConverter.ToDouble(raw, 280)
                ``JOBSEC4`` = BitConverter.ToDouble(raw, 288)
                ``JOBSEC5`` = BitConverter.ToDouble(raw, 296)
                ``JOBSEC6`` = BitConverter.ToDouble(raw, 304)
                ``JOBSEC7`` = BitConverter.ToDouble(raw, 312)
                ``JOBSEC8`` = BitConverter.ToDouble(raw, 320)
                ``JOBSEC9`` = BitConverter.ToDouble(raw, 328)
                ``JOBSEC10`` = BitConverter.ToDouble(raw, 336)
                ``JOBSEC11`` = BitConverter.ToDouble(raw, 344)
                ``JOBSEC12`` = BitConverter.ToDouble(raw, 352)
                ``JOBSEC13`` = BitConverter.ToDouble(raw, 360)
                ``JOBSEC14`` = BitConverter.ToDouble(raw, 368)
                ``JOBSEC15`` = BitConverter.ToDouble(raw, 376)
                ``JOBSEC16`` = BitConverter.ToDouble(raw, 384)
                ``JOBSEC17`` = BitConverter.ToDouble(raw, 392)
                ``JOBSEC18`` = BitConverter.ToDouble(raw, 400)
                ``JOBSEC19`` = BitConverter.ToDouble(raw, 408)
                ``JOBSEC20`` = BitConverter.ToDouble(raw, 416)
                ``TQUIT`` = BitConverter.ToDouble(raw, 424)
                ``ABSENT1`` = BitConverter.ToDouble(raw, 432)
                ``ABSENT2`` = BitConverter.ToDouble(raw, 440)
                ``PHIA1`` = BitConverter.ToDouble(raw, 448)
                ``PHIA2`` = BitConverter.ToDouble(raw, 456)
                ``PHIA3`` = BitConverter.ToDouble(raw, 464)
                ``PHIA4`` = BitConverter.ToDouble(raw, 472)
                ``JOBSAT1`` = BitConverter.ToDouble(raw, 480)
                ``JOBSAT2`` = BitConverter.ToDouble(raw, 488)
                ``JOBSAT3`` = BitConverter.ToDouble(raw, 496)
                ``JOBSAT4`` = BitConverter.ToDouble(raw, 504)
                ``JOBSAT5`` = BitConverter.ToDouble(raw, 512)
                ``JOBSAT6`` = BitConverter.ToDouble(raw, 520)
                ``JOBSAT7`` = BitConverter.ToDouble(raw, 528)
                ``JOBSAT8`` = BitConverter.ToDouble(raw, 536)
                ``JOBSAT9`` = BitConverter.ToDouble(raw, 544)
                ``JOBSAT10`` = BitConverter.ToDouble(raw, 552)
                ``JOBSAT11`` = BitConverter.ToDouble(raw, 560)
                ``JOBSAT12`` = BitConverter.ToDouble(raw, 568)
                ``JOBSAT13`` = BitConverter.ToDouble(raw, 576)
                ``JOBSAT14`` = BitConverter.ToDouble(raw, 584)
                ``JOBSAT15`` = BitConverter.ToDouble(raw, 592)
                ``JOBSAT16`` = BitConverter.ToDouble(raw, 600)
                ``JOBSAT17`` = BitConverter.ToDouble(raw, 608)
                ``JOBSAT18`` = BitConverter.ToDouble(raw, 616)
                ``JOBSAT19`` = BitConverter.ToDouble(raw, 624)
                ``JOBSAT20`` = BitConverter.ToDouble(raw, 632)
                ``PHIB1`` = BitConverter.ToDouble(raw, 640)
                ``PHIB2`` = BitConverter.ToDouble(raw, 648)
                ``QUIT1`` = BitConverter.ToDouble(raw, 656)
                ``QUIT2`` = BitConverter.ToDouble(raw, 664)
                ``QUIT3`` = BitConverter.ToDouble(raw, 672)
                ``ABSENCES`` = byte2str raw 1152 1
                ``PHIC1`` = BitConverter.ToDouble(raw, 680)
                ``PHIC2`` = BitConverter.ToDouble(raw, 688)
                ``PHIC3`` = BitConverter.ToDouble(raw, 696)
                ``PHIC4`` = BitConverter.ToDouble(raw, 704)
                ``PHIC5`` = BitConverter.ToDouble(raw, 712)
                ``PHIC6`` = BitConverter.ToDouble(raw, 720)
                ``PHIC7`` = BitConverter.ToDouble(raw, 728)
                ``PHIC8`` = BitConverter.ToDouble(raw, 736)
                ``PHIC9`` = BitConverter.ToDouble(raw, 744)
                ``PHIC10`` = BitConverter.ToDouble(raw, 752)
                ``PHIC11`` = BitConverter.ToDouble(raw, 760)
                ``PHIC12`` = BitConverter.ToDouble(raw, 768)
                ``PHIC13`` = BitConverter.ToDouble(raw, 776)
                ``PHIC14`` = BitConverter.ToDouble(raw, 784)
                ``TEAM1`` = BitConverter.ToDouble(raw, 792)
                ``TEAM2`` = BitConverter.ToDouble(raw, 800)
                ``TEAM3`` = BitConverter.ToDouble(raw, 808)
                ``PHID1`` = BitConverter.ToDouble(raw, 816)
                ``PHID2`` = BitConverter.ToDouble(raw, 824)
                ``PHID3`` = BitConverter.ToDouble(raw, 832)
                ``PHID4`` = BitConverter.ToDouble(raw, 840)
                ``PHID5`` = BitConverter.ToDouble(raw, 848)
                ``PHID6`` = BitConverter.ToDouble(raw, 856)
                ``PHID7`` = BitConverter.ToDouble(raw, 864)
                ``PHIE1`` = BitConverter.ToDouble(raw, 872)
                ``PHIE2`` = BitConverter.ToDouble(raw, 880)
                ``PHIE3`` = BitConverter.ToDouble(raw, 888)
                ``PHIE4`` = BitConverter.ToDouble(raw, 896)
                ``PHIE5`` = BitConverter.ToDouble(raw, 904)
                ``PHIE6`` = BitConverter.ToDouble(raw, 912)
                ``PHIE7`` = BitConverter.ToDouble(raw, 920)
                ``PHIE8`` = BitConverter.ToDouble(raw, 928)
                ``PHIE9`` = BitConverter.ToDouble(raw, 936)
                ``PHIE10`` = BitConverter.ToDouble(raw, 944)
                ``PHIE11`` = BitConverter.ToDouble(raw, 952)
                ``PHIE12`` = BitConverter.ToDouble(raw, 960)
                ``DIVISION`` = BitConverter.ToDouble(raw, 968)
                ``AGE`` = BitConverter.ToDouble(raw, 976)
                ``SEX`` = BitConverter.ToDouble(raw, 984)
                ``RACE`` = BitConverter.ToDouble(raw, 992)
                ``MSTATUS`` = BitConverter.ToDouble(raw, 1000)
                ``NCHILD`` = BitConverter.ToDouble(raw, 1008)
                ``EDUC`` = BitConverter.ToDouble(raw, 1016)
                ``POSITION`` = BitConverter.ToDouble(raw, 1024)
                ``SATSCHED`` = BitConverter.ToDouble(raw, 1032)
                ``TYPESCH`` = BitConverter.ToDouble(raw, 1040)
                ``FLEX`` = BitConverter.ToDouble(raw, 1048)
                ``URBRUR`` = BitConverter.ToDouble(raw, 1056)
                ``HOURS`` = BitConverter.ToDouble(raw, 1064)
                ``SCHOOL`` = BitConverter.ToDouble(raw, 1072)
                ``ANJOB`` = BitConverter.ToDouble(raw, 1080)
                ``TYPERES`` = BitConverter.ToDouble(raw, 1088)
                ``LENGTHE`` = BitConverter.ToDouble(raw, 1096)
                ``HOWLONG`` = BitConverter.ToDouble(raw, 1104)
                ``REASONQ`` = BitConverter.ToDouble(raw, 1112)
                ``REASONW`` = BitConverter.ToDouble(raw, 1120)
                ``WHY`` = BitConverter.ToDouble(raw, 1128)
                ``NHOURS`` = BitConverter.ToDouble(raw, 1136)
                ``EMP_ID`` = BitConverter.ToDouble(raw, 1144)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*PIZZAZZ.rowSize .. dataStart+(i+1)*PIZZAZZ.rowSize-1]) // chop page 
                |> Array.map PIZZAZZ.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*PIZZAZZ.rowSize .. dataStart+(i+1)*PIZZAZZ.rowSize-1]) // chop page 
                |> Array.map PIZZAZZ.fromRaw
            | _ -> Array.empty<PIZZAZZ>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\pizzazz.sas7bdat"
            for pp in p do
                yield! PIZZAZZ.p2a pp
        }    

    type POLICE = {
        ``ID`` : float
        ``REACT`` : float
        ``HEIGHT`` : float
        ``WEIGHT`` : float
        ``SHLDR`` : float
        ``PELVIC`` : float
        ``CHEST`` : float
        ``THIGH`` : float
        ``PULSE`` : float
        ``DIAST`` : float
        ``CHNUP`` : float
        ``BREATH`` : float
        ``RECVR`` : float
        ``SPEED`` : float
        ``ENDUR`` : float
        ``FAT`` : float

    }
    with 
        static member rowSize = 128
        static member fromRaw (raw:byte[]) = {
                ``ID`` = BitConverter.ToDouble(raw, 0)
                ``REACT`` = BitConverter.ToDouble(raw, 8)
                ``HEIGHT`` = BitConverter.ToDouble(raw, 16)
                ``WEIGHT`` = BitConverter.ToDouble(raw, 24)
                ``SHLDR`` = BitConverter.ToDouble(raw, 32)
                ``PELVIC`` = BitConverter.ToDouble(raw, 40)
                ``CHEST`` = BitConverter.ToDouble(raw, 48)
                ``THIGH`` = BitConverter.ToDouble(raw, 56)
                ``PULSE`` = BitConverter.ToDouble(raw, 64)
                ``DIAST`` = BitConverter.ToDouble(raw, 72)
                ``CHNUP`` = BitConverter.ToDouble(raw, 80)
                ``BREATH`` = BitConverter.ToDouble(raw, 88)
                ``RECVR`` = BitConverter.ToDouble(raw, 96)
                ``SPEED`` = BitConverter.ToDouble(raw, 104)
                ``ENDUR`` = BitConverter.ToDouble(raw, 112)
                ``FAT`` = BitConverter.ToDouble(raw, 120)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*POLICE.rowSize .. dataStart+(i+1)*POLICE.rowSize-1]) // chop page 
                |> Array.map POLICE.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*POLICE.rowSize .. dataStart+(i+1)*POLICE.rowSize-1]) // chop page 
                |> Array.map POLICE.fromRaw
            | _ -> Array.empty<POLICE>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\police.sas7bdat"
            for pp in p do
                yield! POLICE.p2a pp
        }    

    type POOR = {
        ``c`` : float
        ``x`` : float
        ``y`` : float

    }
    with 
        static member rowSize = 24
        static member fromRaw (raw:byte[]) = {
                ``c`` = BitConverter.ToDouble(raw, 0)
                ``x`` = BitConverter.ToDouble(raw, 8)
                ``y`` = BitConverter.ToDouble(raw, 16)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*POOR.rowSize .. dataStart+(i+1)*POOR.rowSize-1]) // chop page 
                |> Array.map POOR.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*POOR.rowSize .. dataStart+(i+1)*POOR.rowSize-1]) // chop page 
                |> Array.map POOR.fromRaw
            | _ -> Array.empty<POOR>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\poor.sas7bdat"
            for pp in p do
                yield! POOR.p2a pp
        }    

    type PROTEIN = {
        ``Country`` : string
        ``RedMeat`` : float
        ``WhiteMeat`` : float
        ``Eggs`` : float
        ``Milk`` : float
        ``Fish`` : float
        ``Cereal`` : float
        ``Starch`` : float
        ``Nuts`` : float
        ``FruitVeg`` : float

    }
    with 
        static member rowSize = 96
        static member fromRaw (raw:byte[]) = {
                ``Country`` = byte2str raw 72 18
                ``RedMeat`` = BitConverter.ToDouble(raw, 0)
                ``WhiteMeat`` = BitConverter.ToDouble(raw, 8)
                ``Eggs`` = BitConverter.ToDouble(raw, 16)
                ``Milk`` = BitConverter.ToDouble(raw, 24)
                ``Fish`` = BitConverter.ToDouble(raw, 32)
                ``Cereal`` = BitConverter.ToDouble(raw, 40)
                ``Starch`` = BitConverter.ToDouble(raw, 48)
                ``Nuts`` = BitConverter.ToDouble(raw, 56)
                ``FruitVeg`` = BitConverter.ToDouble(raw, 64)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*PROTEIN.rowSize .. dataStart+(i+1)*PROTEIN.rowSize-1]) // chop page 
                |> Array.map PROTEIN.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*PROTEIN.rowSize .. dataStart+(i+1)*PROTEIN.rowSize-1]) // chop page 
                |> Array.map PROTEIN.fromRaw
            | _ -> Array.empty<PROTEIN>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\protein.sas7bdat"
            for pp in p do
                yield! PROTEIN.p2a pp
        }    

    type RAT = {
        ``RAT`` : string
        ``DIET`` : string
        ``CHOL`` : float
        ``FIBER`` : float
        ``TLIP`` : float
        ``TCHOL`` : float
        ``FWW`` : float
        ``FWD`` : float
        ``BLK`` : float
        ``TPCHOL`` : float
        ``HDL`` : float
        ``PCTDRYMT`` : float

    }
    with 
        static member rowSize = 88
        static member fromRaw (raw:byte[]) = {
                ``RAT`` = byte2str raw 80 3
                ``DIET`` = byte2str raw 83 1
                ``CHOL`` = BitConverter.ToDouble(raw, 0)
                ``FIBER`` = BitConverter.ToDouble(raw, 8)
                ``TLIP`` = BitConverter.ToDouble(raw, 16)
                ``TCHOL`` = BitConverter.ToDouble(raw, 24)
                ``FWW`` = BitConverter.ToDouble(raw, 32)
                ``FWD`` = BitConverter.ToDouble(raw, 40)
                ``BLK`` = BitConverter.ToDouble(raw, 48)
                ``TPCHOL`` = BitConverter.ToDouble(raw, 56)
                ``HDL`` = BitConverter.ToDouble(raw, 64)
                ``PCTDRYMT`` = BitConverter.ToDouble(raw, 72)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*RAT.rowSize .. dataStart+(i+1)*RAT.rowSize-1]) // chop page 
                |> Array.map RAT.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*RAT.rowSize .. dataStart+(i+1)*RAT.rowSize-1]) // chop page 
                |> Array.map RAT.fromRaw
            | _ -> Array.empty<RAT>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\rat.sas7bdat"
            for pp in p do
                yield! RAT.p2a pp
        }    

    type STORAGE = {
        ``REP`` : float
        ``TRT`` : float
        ``CKG_LOSS`` : float
        ``PH`` : float
        ``MOIST`` : float
        ``FAT`` : float
        ``HEX`` : float
        ``NONHEM`` : float
        ``CKG_TIME`` : float

    }
    with 
        static member rowSize = 72
        static member fromRaw (raw:byte[]) = {
                ``REP`` = BitConverter.ToDouble(raw, 0)
                ``TRT`` = BitConverter.ToDouble(raw, 8)
                ``CKG_LOSS`` = BitConverter.ToDouble(raw, 16)
                ``PH`` = BitConverter.ToDouble(raw, 24)
                ``MOIST`` = BitConverter.ToDouble(raw, 32)
                ``FAT`` = BitConverter.ToDouble(raw, 40)
                ``HEX`` = BitConverter.ToDouble(raw, 48)
                ``NONHEM`` = BitConverter.ToDouble(raw, 56)
                ``CKG_TIME`` = BitConverter.ToDouble(raw, 64)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*STORAGE.rowSize .. dataStart+(i+1)*STORAGE.rowSize-1]) // chop page 
                |> Array.map STORAGE.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*STORAGE.rowSize .. dataStart+(i+1)*STORAGE.rowSize-1]) // chop page 
                |> Array.map STORAGE.fromRaw
            | _ -> Array.empty<STORAGE>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\storage.sas7bdat"
            for pp in p do
                yield! STORAGE.p2a pp
        }    

    type STORES = {
        ``rep`` : float
        ``size`` : string
        ``location`` : string
        ``meat`` : float
        ``produce`` : float
        ``cleaning`` : float
        ``housewrs`` : float
        ``petwares`` : float
        ``other`` : float

    }
    with 
        static member rowSize = 80
        static member fromRaw (raw:byte[]) = {
                ``rep`` = BitConverter.ToDouble(raw, 0)
                ``size`` = byte2str raw 56 9
                ``location`` = byte2str raw 65 13
                ``meat`` = BitConverter.ToDouble(raw, 8)
                ``produce`` = BitConverter.ToDouble(raw, 16)
                ``cleaning`` = BitConverter.ToDouble(raw, 24)
                ``housewrs`` = BitConverter.ToDouble(raw, 32)
                ``petwares`` = BitConverter.ToDouble(raw, 40)
                ``other`` = BitConverter.ToDouble(raw, 48)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*STORES.rowSize .. dataStart+(i+1)*STORES.rowSize-1]) // chop page 
                |> Array.map STORES.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*STORES.rowSize .. dataStart+(i+1)*STORES.rowSize-1]) // chop page 
                |> Array.map STORES.fromRaw
            | _ -> Array.empty<STORES>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\stores.sas7bdat"
            for pp in p do
                yield! STORES.p2a pp
        }    

    type TURKEY1 = {
        ``ID`` : string
        ``S`` : float
        ``T`` : float
        ``SIDE`` : string
        ``WGT`` : float
        ``HUM`` : float
        ``RAD`` : float
        ``ULN`` : float
        ``FEMUR`` : float
        ``TIB`` : float
        ``TIN`` : float
        ``CAR`` : float
        ``D3P`` : float
        ``STL`` : float
        ``STB`` : float
        ``COR`` : float
        ``PEL`` : float
        ``MAX`` : float
        ``MIN`` : float
        ``SCA`` : float
        ``Sex`` : string
        ``Type`` : string

    }
    with 
        static member rowSize = 168
        static member fromRaw (raw:byte[]) = {
                ``ID`` = byte2str raw 144 5
                ``S`` = BitConverter.ToDouble(raw, 0)
                ``T`` = BitConverter.ToDouble(raw, 8)
                ``SIDE`` = byte2str raw 149 1
                ``WGT`` = BitConverter.ToDouble(raw, 16)
                ``HUM`` = BitConverter.ToDouble(raw, 24)
                ``RAD`` = BitConverter.ToDouble(raw, 32)
                ``ULN`` = BitConverter.ToDouble(raw, 40)
                ``FEMUR`` = BitConverter.ToDouble(raw, 48)
                ``TIB`` = BitConverter.ToDouble(raw, 56)
                ``TIN`` = BitConverter.ToDouble(raw, 64)
                ``CAR`` = BitConverter.ToDouble(raw, 72)
                ``D3P`` = BitConverter.ToDouble(raw, 80)
                ``STL`` = BitConverter.ToDouble(raw, 88)
                ``STB`` = BitConverter.ToDouble(raw, 96)
                ``COR`` = BitConverter.ToDouble(raw, 104)
                ``PEL`` = BitConverter.ToDouble(raw, 112)
                ``MAX`` = BitConverter.ToDouble(raw, 120)
                ``MIN`` = BitConverter.ToDouble(raw, 128)
                ``SCA`` = BitConverter.ToDouble(raw, 136)
                ``Sex`` = byte2str raw 150 6
                ``Type`` = byte2str raw 156 8
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*TURKEY1.rowSize .. dataStart+(i+1)*TURKEY1.rowSize-1]) // chop page 
                |> Array.map TURKEY1.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*TURKEY1.rowSize .. dataStart+(i+1)*TURKEY1.rowSize-1]) // chop page 
                |> Array.map TURKEY1.fromRaw
            | _ -> Array.empty<TURKEY1>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\turkey1.sas7bdat"
            for pp in p do
                yield! TURKEY1.p2a pp
        }    

    type TURKEY2 = {
        ``ID`` : float
        ``TYPE`` : string
        ``SEX`` : string
        ``AGE`` : string
        ``HUM`` : string
        ``RAD`` : string
        ``ULN`` : string
        ``CAR`` : string
        ``FEM`` : string
        ``TIB`` : string
        ``TIN`` : string
        ``MAX`` : string
        ``MIN`` : string
        ``MMS`` : string
        ``BC`` : string
        ``STL`` : string
        ``STB`` : string
        ``KEL`` : string
        ``GB`` : string
        ``ANL`` : string
        ``HN`` : string
        ``TK`` : string
        ``WBL`` : string

    }
    with 
        static member rowSize = 96
        static member fromRaw (raw:byte[]) = {
                ``ID`` = BitConverter.ToDouble(raw, 0)
                ``TYPE`` = byte2str raw 8 2
                ``SEX`` = byte2str raw 10 1
                ``AGE`` = byte2str raw 11 2
                ``HUM`` = byte2str raw 13 3
                ``RAD`` = byte2str raw 16 3
                ``ULN`` = byte2str raw 19 3
                ``CAR`` = byte2str raw 22 2
                ``FEM`` = byte2str raw 24 3
                ``TIB`` = byte2str raw 27 3
                ``TIN`` = byte2str raw 30 15
                ``MAX`` = byte2str raw 45 7
                ``MIN`` = byte2str raw 52 7
                ``MMS`` = byte2str raw 59 3
                ``BC`` = byte2str raw 62 3
                ``STL`` = byte2str raw 65 3
                ``STB`` = byte2str raw 68 3
                ``KEL`` = byte2str raw 71 6
                ``GB`` = byte2str raw 77 3
                ``ANL`` = byte2str raw 80 3
                ``HN`` = byte2str raw 83 3
                ``TK`` = byte2str raw 86 3
                ``WBL`` = byte2str raw 89 3
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*TURKEY2.rowSize .. dataStart+(i+1)*TURKEY2.rowSize-1]) // chop page 
                |> Array.map TURKEY2.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*TURKEY2.rowSize .. dataStart+(i+1)*TURKEY2.rowSize-1]) // chop page 
                |> Array.map TURKEY2.fromRaw
            | _ -> Array.empty<TURKEY2>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\turkey2.sas7bdat"
            for pp in p do
                yield! TURKEY2.p2a pp
        }    

    type UNEQUAL = {
        ``c`` : float
        ``x`` : float
        ``y`` : float

    }
    with 
        static member rowSize = 24
        static member fromRaw (raw:byte[]) = {
                ``c`` = BitConverter.ToDouble(raw, 0)
                ``x`` = BitConverter.ToDouble(raw, 8)
                ``y`` = BitConverter.ToDouble(raw, 16)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*UNEQUAL.rowSize .. dataStart+(i+1)*UNEQUAL.rowSize-1]) // chop page 
                |> Array.map UNEQUAL.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*UNEQUAL.rowSize .. dataStart+(i+1)*UNEQUAL.rowSize-1]) // chop page 
                |> Array.map UNEQUAL.fromRaw
            | _ -> Array.empty<UNEQUAL>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\unequal.sas7bdat"
            for pp in p do
                yield! UNEQUAL.p2a pp
        }    

    type USNAVY = {
        ``SITE`` : float
        ``ADO`` : float
        ``MAC`` : float
        ``WHR`` : float
        ``CUA`` : float
        ``WNGS`` : float
        ``OBC`` : float
        ``RMS`` : float
        ``MMH`` : float

    }
    with 
        static member rowSize = 72
        static member fromRaw (raw:byte[]) = {
                ``SITE`` = BitConverter.ToDouble(raw, 0)
                ``ADO`` = BitConverter.ToDouble(raw, 8)
                ``MAC`` = BitConverter.ToDouble(raw, 16)
                ``WHR`` = BitConverter.ToDouble(raw, 24)
                ``CUA`` = BitConverter.ToDouble(raw, 32)
                ``WNGS`` = BitConverter.ToDouble(raw, 40)
                ``OBC`` = BitConverter.ToDouble(raw, 48)
                ``RMS`` = BitConverter.ToDouble(raw, 56)
                ``MMH`` = BitConverter.ToDouble(raw, 64)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*USNAVY.rowSize .. dataStart+(i+1)*USNAVY.rowSize-1]) // chop page 
                |> Array.map USNAVY.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*USNAVY.rowSize .. dataStart+(i+1)*USNAVY.rowSize-1]) // chop page 
                |> Array.map USNAVY.fromRaw
            | _ -> Array.empty<USNAVY>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\usnavy.sas7bdat"
            for pp in p do
                yield! USNAVY.p2a pp
        }    

    type WHEAT = {
        ``DOWN_A`` : float
        ``DOWN_P`` : float
        ``DOWN_L`` : float
        ``DOWN_B`` : float
        ``LOC`` : string
        ``VARIETY`` : string
        ``GRP`` : float
        ``RIGHT_A`` : float
        ``RIGHT_P`` : float
        ``RIGHT_L`` : float
        ``RIGHT_B`` : float

    }
    with 
        static member rowSize = 88
        static member fromRaw (raw:byte[]) = {
                ``DOWN_A`` = BitConverter.ToDouble(raw, 0)
                ``DOWN_P`` = BitConverter.ToDouble(raw, 8)
                ``DOWN_L`` = BitConverter.ToDouble(raw, 16)
                ``DOWN_B`` = BitConverter.ToDouble(raw, 24)
                ``LOC`` = byte2str raw 72 6
                ``VARIETY`` = byte2str raw 78 6
                ``GRP`` = BitConverter.ToDouble(raw, 32)
                ``RIGHT_A`` = BitConverter.ToDouble(raw, 40)
                ``RIGHT_P`` = BitConverter.ToDouble(raw, 48)
                ``RIGHT_L`` = BitConverter.ToDouble(raw, 56)
                ``RIGHT_B`` = BitConverter.ToDouble(raw, 64)
    
        }
        static member p2a (p:Page) = 
            match p with 
            | Mix (shs, bc, raw ) -> 
                let shCount = shs.Length
                let shpEnd = 24 + shCount * 12
                let rem = shpEnd  % 8     
                let dataStart = shpEnd + if rem > 0 then 8 - rem else 0 // round up to next 8 bytes
                Array.init (bc-shCount) (fun i -> raw.[dataStart+i*WHEAT.rowSize .. dataStart+(i+1)*WHEAT.rowSize-1]) // chop page 
                |> Array.map WHEAT.fromRaw
            | Data (bc, raw) -> 
                let dataStart = 24
                Array.init bc (fun i -> raw.[dataStart+i*WHEAT.rowSize .. dataStart+(i+1)*WHEAT.rowSize-1]) // chop page 
                |> Array.map WHEAT.fromRaw
            | _ -> Array.empty<WHEAT>
        static member Data = seq {
            let (h,p) = fileHeaderAndPages @"D:\SAS Files\wheat.sas7bdat"
            for pp in p do
                yield! WHEAT.p2a pp
        }    

