// --------------------------------------------------------------------------------------
// The Bank for International Settlements (BIS) type provider 
// --------------------------------------------------------------------------------------

namespace ProviderImplementation

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open ProviderImplementation
open ProviderImplementation.ProvidedTypes

open FSharp.Data.Runtime.Bis

// --------------------------------------------------------------------------------------

[<TypeProvider>]
type public BisProvider(cfg:TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
    
    let asm = Assembly.GetExecutingAssembly()
    let ns = "FSharp.Data.Toolbox.Bis"

    // Create the types based on a BIS dataset file.
    let createTypes () =        
        let datasetProvider = new ProvidedTypeDefinition(asm, ns, "Dataset", Some typeof<obj>)
        
        // Parameter for the BIS dataset file.
        datasetProvider.DefineStaticParameters([ProvidedStaticParameter("pathToBisFile", typeof<string>)], fun typeName args ->
            let pathToDatasetFile = args.[0] :?> string

            // Create parser based on the datset file
            let parser = createPraser pathToDatasetFile

            // Determine all dimensions out of dataset file
            let dimensionTypes =  
                let dimesionTys = 
                    parser.getDataset().dimensions
                        |> Seq.map (fun d -> 
                                        let p = ProvidedTypeDefinition(d.name, Some typeof<obj>, HideObjectMethods = true)
                                        d.members
                                            |> Seq.map (fun m -> ProvidedProperty(m, typeof<string>, IsStatic = true, GetterCode = fun args -> <@@ m.Substring(0, m.IndexOf(':')) @@> ))
                                            |> Seq.iter (fun m -> p.AddMember(m))
                                        p)
                        |> Seq.toList

                dimesionTys
            
            // Generate an observation filter based on dataset file
            let filterProvider =
                let dset = parser.getDataset()

                let filterTy = ProvidedTypeDefinition("ObservationFilter", Some typeof<obj>, HideObjectMethods = true)
                filterTy.AddMember <| ProvidedConstructor(parameters = [], InvokeCode = fun args -> <@@ new Dictionary<string, string list>() @@>)

                // Generate property for each dimension in dataset
                dset.dimensions.Select(fun x -> x.name)
                    |> Seq.map (fun d -> 
                                    ProvidedProperty (
                                        d, 
                                        typeof<string list>, 
                                        IsStatic = false, 
                                        GetterCode = (fun args -> 
                                                        <@@ let dict = ((%%args.[0] : obj) :?> Dictionary<string,string list>)
                                                            if not (dict.ContainsKey d) then dict.Add (d, [])
                                                            dict.[d]
                                                         @@>),
                                        SetterCode = (fun args -> 
                                                        <@@ ((%%args.[0] : obj) :?>Dictionary<string,string list>).[d] <- (%%args.[1] : string list) @@>)))
                    |> Seq.toList
                    |> filterTy.AddMembers

                // Method that applies the filter on a dataset file and returns the matching observations
                let getFilterMeth = ProvidedMethod("Get", [ProvidedParameter("pathToBisFile", typeof<string>, optionalValue = true)], typeof<Observation list>)
                getFilterMeth.InvokeCode <- (fun args -> 
                                    <@@ 
                                        let dict = ((%%args.[0] : obj) :?> System.Collections.Generic.Dictionary<string,string list>)
                                        let filePath = pathToDatasetFile
                                        //let filePath = if args.Length = 2 then (%%args.[1] :?> string) else pathToDatasetFile
                                        let obsFilter = new Dictionary<string, string list>()
                                        for f in dict.Where((fun d -> d.Value.Length > 0)) do
                                            obsFilter.Add(f.Key, f.Value)

                                        let fileParser = createPraser filePath
                                        fileParser.filter (obsFilter)
                                     @@>)
            
                filterTy.AddMember (getFilterMeth)
                filterTy

            let provider = new ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
            provider.AddMember <| ProvidedConstructor(parameters = [], InvokeCode = fun _ -> <@@ new Dictionary<string, string list>() @@>)
            let alltypes = dimensionTypes.Union([filterProvider]).ToList() 
            provider.AddMembers(alltypes |> Seq.toList)
            provider)

        [datasetProvider]

    do this.AddNamespace(ns, createTypes())

[<assembly:TypeProviderAssembly>]
 do()