module Nebula.ApiTypes 

open System.Dynamic
open System.Collections.Generic
open System
open System.Runtime.Serialization

type Object = 
| StringValue of string
| DateValue of DateTime
| Int64Value of int64
| IntValue of int
| DecimalValue of decimal
| BoolValue of bool
| MapValue of Map<string, Object>
| ListValue of Object list
| Empty of unit

let (?) (target:Object) (name:string) : Object =
    match target with
    | MapValue v -> match v.ContainsKey(name) with
                    | true -> v.[name]
                    | false -> Empty ()
    | _ -> target

let (?>) (target:Object) (name:string) : Object list =
    match target with
    | Empty v -> []
    | MapValue v -> 
        match v.[name] with
        | ListValue vv -> vv
        | _ -> []
    | _ -> []

type XmlEveError = 
    { Code:int; Message:string }

type XmlEveResponse = 
    { Result:Object option; 
      Version: string; 
      CurrentTime:DateTime; 
      CachedUntil:DateTime;
      Error: XmlEveError option}
        
