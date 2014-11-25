module Nebula.SDE.Items

open FSharp.Data
open System.IO

type ItemsProvider = CsvProvider<"typeID|groupID|typeName|mass|volume|capacity|portionSize|raceID|basePrice|published|marketGroupID|chanceOfDuplicating", Separators="|">
type InvType = 
    { TypeId:int64; TypeName:string }
    override x.ToString() =
        let t = x.GetType()
        (t.GetProperties() 
        |> Array.fold (fun acc prop -> acc + prop.Name + "=" + string(prop.GetValue(x, null)) + ";") "").TrimEnd(';')

let items = lazy(
                let fileName = "SDE/items.csv"
                if File.Exists(fileName) then 
                    let items = ItemsProvider.Load(fileName)
                    items.Rows
                    |> Seq.map (fun x -> { TypeId = int64(x.typeID); TypeName = x.typeName })
                    |> List.ofSeq
                    |> Some
                else None
            )

let getItems() = items.Value 
let getItem typeId =
    match getItems() with
    | Some(list) ->
        List.tryFind (fun x -> x.TypeId = typeId) list
    | _ -> None