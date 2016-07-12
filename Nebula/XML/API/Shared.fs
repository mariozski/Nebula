module Nebula.XML.API.Shared

open Nebula.ApiTypes

type TypeId = int64
type LocationId = int64

let internal genericToString (x:obj) = 
    let t = x.GetType()
    (t.GetProperties() |> Array.fold (fun acc prop -> acc + prop.Name + "=" + string(prop.GetValue(x, null)) + ";") "").TrimEnd(';')

let handleResult (xmlResult:XmlEveResponse) handler = 
        match xmlResult.Result with
        | Some(result) ->
            handler(result)
        | None -> failwith "Error retrieving result from XML"


