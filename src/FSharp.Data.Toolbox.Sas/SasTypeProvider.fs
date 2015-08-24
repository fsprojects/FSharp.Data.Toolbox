namespace FSharp.Data.Toolbox.SasFile 

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type public SasProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "FSharp.Data.Toolbox.SasProvider"
    let asm = Assembly.GetExecutingAssembly()

    // Create the main provided type
    let tySasFile = ProvidedTypeDefinition(asm, ns, "SasFile", Some typeof<obj>) 

    // Parameterize the type by the file to use as a template
    let filename = ProvidedStaticParameter("filename", typeof<string>)

    do
        tySasFile.DefineStaticParameters(
            [filename], fun tyName [| :? string as filename |] ->

        let filename = 
            if not <| System.IO.File.Exists filename then
                // resolve the filename relative to the resolution folder
                let resolvedFilename = System.IO.Path.Combine(config.ResolutionFolder, filename)
                if not <| System.IO.File.Exists resolvedFilename then
                    failwithf "File '%s' not found" resolvedFilename 
                resolvedFilename 
            else 
                filename

        // read SAS schema
        let sasFile = new SasFile(filename)
        let header = sasFile.Header
        let meta = sasFile.MetaData

        tySasFile.AddMember(
            ProvidedProperty("Header", typeof<Header>,
                GetterCode = fun args -> <@@ (%%(args.[0]) :> obj) :?> Header @@>) )
        
        tySasFile.AddMember(
            ProvidedProperty("MetaData", typeof<MetaData>,
                GetterCode = fun args -> <@@ (%%(args.[0]) :> obj) :?> MetaData @@>) )

        tySasFile.AddMember(
            ProvidedProperty("Path", typeof<string>,
                GetterCode = fun args -> <@@ (%%(args.[0]) :> obj) :?> string @@>) )

        // define a provided type for each row, erasing to a SasFile.Core.Value list
        let tyRow = ProvidedTypeDefinition("Row", Some(typeof<Value list>))
        
        let columns = meta.Columns

        // add one property per SAS variable
        let count = List.length columns
        for i = 0 to count - 1 do
            let prop = ProvidedProperty(columns.[i].Name, typeof<string>,
                        GetterCode = fun [values] ->
                                        <@@ (%%values: Value list).[i] @@>
                        )
            // Add metadata defining the property's location in the referenced file
            prop.AddDefinitionLocation(1, columns.[i].Ordinal, filename)
            tyRow.AddMember prop


        // define the provided type, erasing to SasFile
        let ty = ProvidedTypeDefinition(asm, ns, tyName, Some typeof<SasFile>)

        // add a parameterless constructor which loads the file that was used to define the schema
        ty.AddMember(ProvidedConstructor([], InvokeCode = fun [] -> <@@ new SasFile(filename) @@>))

        // add a constructor taking the filename to load
        ty.AddMember(ProvidedConstructor(
                        [ProvidedParameter("filename", typeof<string>)],
                        InvokeCode = fun [filename] -> <@@ new SasFile(%%filename) @@>))

        // add a new, more strongly typed Data property (which uses the existing property at runtime)
        ty.AddMember(ProvidedProperty(
                        "MetaData", typeof<MetaData>, //typedefof<seq<_>>.MakeGenericType(rowTy), 
                        GetterCode = fun [sasFile] -> <@@ (%%sasFile: SasFile).MetaData @@>))

        // add the row type as a nested type
        ty.AddMember tyRow

        ty.AddXmlDocDelayed (fun () -> "Provided type 'SasFile'")
        ty
        )

    do
        this.AddNamespace(ns, [ tySasFile ] )

[<assembly:TypeProviderAssembly>]
do ()


