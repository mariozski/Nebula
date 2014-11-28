module Nebula.SDE.Items

open FSharp.Data
open System.IO
open System.Resources
open System.Reflection
open System.IO.Compression

type ItemsProvider = CsvProvider<""""typeID","groupID","typeName","description","mass","volume","capacity","portionSize","raceID","basePrice","published","marketGroupID","chanceOfDuplicating" """, HasHeaders=true>
type InvType = 
    { TypeId:int64; TypeName:string }
    override x.ToString() =
        let t = x.GetType()
        (t.GetProperties() 
        |> Array.fold (fun acc prop -> acc + prop.Name + "=" + string(prop.GetValue(x, null)) + ";") "").TrimEnd(';')

let private items = lazy(
                let resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("invTypes.csv.gz")
                use gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress)
                let items = ItemsProvider.Load(gzipStream)
                items.Rows
                |> Seq.map (fun x -> { TypeId = int64(x.TypeID); TypeName = x.TypeName })
                |> List.ofSeq
            )

let getItems() = 
    items.Value

let getItem typeId = 
    getItems()
    |> List.tryFind (fun x -> x.TypeId = typeId) 