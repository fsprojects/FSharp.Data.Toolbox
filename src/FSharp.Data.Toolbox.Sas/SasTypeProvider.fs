namespace FSharp.Data.Toolbox.SasFile

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type public SasProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "FSharp.Data.Toolbox.SasProvider"
    let asm = Assembly.GetExecutingAssembly()

    let createTypes () =
        // Create the main provided type
        let tySasFile = ProvidedTypeDefinition(asm, ns, "SasFile", Some typeof<obj>)

        // Parameterize the type by the file to use as a template
        let filename = ProvidedStaticParameter("filename", typeof<string>)

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

            // define a provided type for each row, erasing to a SasFile.Core.Value seq
            let tyRow = ProvidedTypeDefinition("Row", Some typeof<Value seq>)
           
            let columns = sasFile.MetaData.Columns

            // add one property per SAS variable
            let count = List.length columns
            for i = 0 to count - 1 do
                let prop = ProvidedProperty(columns.[i].Name, typeof<Value>,
                            GetterCode = fun [values] ->
                                            <@@ (Seq.nth i (%%values: Value seq) ) @@>
                            )
                // Add metadata defining the property's location in the referenced file
                //prop.AddDefinitionLocation(1, columns.[i].Ordinal, filename)
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
                            "Data", typedefof<seq<_>>.MakeGenericType(tyRow),
                            GetterCode = fun [sasFile] -> <@@ (%%sasFile: SasFile).Rows @@>))

            // add the row type as a nested type
            ty.AddMember tyRow

            ty.AddXmlDocDelayed (fun () -> "Provided type 'SasFile'")
            ty
            )
        [ tySasFile ]

    do
        this.AddNamespace(ns, createTypes() )

[<assembly:TypeProviderAssembly>]
do ()


