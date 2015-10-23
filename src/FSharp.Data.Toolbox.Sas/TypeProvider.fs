namespace FSharp.Data.Toolbox.Sas

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

#nowarn "25"

[<TypeProvider>]
type public SasProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let ns = "FSharp.Data.Toolbox.Sas"
    let asm = Assembly.GetExecutingAssembly()
    let sasFileTypeProviderName = "SasFileTypeProvider"
    let sasLibraryTypeProviderName = "SasLibraryTypeProvider"

    let createTypes () =
        // Create the main provided type
        let tySasFile = ProvidedTypeDefinition(asm, ns, sasFileTypeProviderName, None)

        // Parameterize the type by the file to use as a template
        let filename = ProvidedStaticParameter("filename", typeof<string>)

        tySasFile.DefineStaticParameters(
            [filename], fun tyName [| :? string as filename |] ->

            let filename' =
                if not <| System.IO.File.Exists filename then
                    // resolve the filename relative to the resolution folder
                    let resolvedFilename = System.IO.Path.Combine(config.ResolutionFolder, filename)
                    if not <| System.IO.File.Exists resolvedFilename then
                        failwithf "File '%s' not found" resolvedFilename
                    resolvedFilename
                else
                    filename 

            // define the provided type, erasing to SasFile
            let ty = ProvidedTypeDefinition(asm, ns, tyName, Some typeof<SasFile>)

            // add a parameterless constructor which loads the file that was used to define the schema
            ty.AddMember(ProvidedConstructor([], InvokeCode = fun [] -> <@@ new SasFile(filename') @@>))

            // add a constructor taking the filename to load
            ty.AddMember(ProvidedConstructor(
                            [ProvidedParameter("filename", typeof<string>)],
                            InvokeCode = fun [filename] -> <@@ new SasFile(%%filename) @@>))



            // define a provided type for each row, erasing to a Value seq
            let tyObservation = ProvidedTypeDefinition("Observation", Some typeof<Value seq>)
       
            // read SAS schema
            use sasFile = new SasFile(filename')

            // add one property per SAS variable
            sasFile.MetaData.Columns
            |> Seq.map (fun col ->
                let i = col.Ordinal - 1 
                ProvidedProperty(col.Name, typeof<Value>,
                    GetterCode = fun [values] ->
                                    <@@ (Seq.nth i (%%values: Value seq) ) @@> ) 
                )
            |> Seq.toList
            |> tyObservation.AddMembers

            // add a new, more strongly typed Data property (which uses the existing property at runtime)
            ty.AddMember(ProvidedProperty(
                            "Observations", typedefof<seq<_>>.MakeGenericType(tyObservation),
                            GetterCode = fun [sasFile] -> <@@ (%%sasFile: SasFile).Rows @@>))

            // add the row type as a nested type
            ty.AddMember tyObservation

            ty.AddXmlDocDelayed (fun () -> sprintf "Provided type '%s'" sasFileTypeProviderName)
            ty
            )
        [ tySasFile ]

    do
        this.AddNamespace(ns, createTypes() )

[<assembly:TypeProviderAssembly>]
do ()


