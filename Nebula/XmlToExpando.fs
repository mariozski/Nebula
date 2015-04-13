module Nebula.XmlToExpando

open System
open System.Globalization
open System.Collections.Generic
open System.Xml.Linq
open System.Dynamic
open System.IO
open Nebula.ApiTypes
open Microsoft.FSharp.Reflection

/// capitalize first letter of string
let capitalize (text:string) =
    Char.ToUpperInvariant(text.[0]).ToString() + text.Substring(1, text.Length - 1)

let xn a = XName.Get(a)

type ParsedValue = 
| StringValue of string
| DateValue of DateTime
| Int64Value of int64
| IntValue of int
| DecimalValue of decimal
| BoolValue of bool

let mi x =
    match x with
    | IntValue v -> v
    | _ -> failwithf "Cannot match %A to int" x

let mi64 x =
    match x with
    | Int64Value v -> v
    | IntValue v -> int64 v
    | _ -> failwithf "Cannot match %A to int64" x

let ms x =
    match x with
    | StringValue v -> v
    | _ as v -> v.ToString()

let md x =
    match x with
    | DecimalValue v -> v
    | IntValue v -> decimal v
    | Int64Value v -> decimal v
    | _ -> failwithf "Cannot match %A to decimal" x

let mb x =
    match x with
    | BoolValue v -> v
    | IntValue v -> 
        match v with
        | 0 -> false
        | _ -> true
    | StringValue v ->
        match v with
        | "True" | "true" -> true
        | "False" | "false" -> false
        | _ -> failwithf "Cannot match %A to bool" v
    | _ -> failwithf "Cannot match %A to bool" x

let mdt x =
    match x with 
    | DateValue v -> v
    | _ -> failwithf "Cannot match %A to date" x

/// Parse value from XML based on format
let parseValue value : ParsedValue = 
    let date = ref DateTime.Now
    let int64 = ref 0L
    let int = ref 0
    let decimal = ref 0M
    let bool = ref false

    if Int32.TryParse(value, int) = true then
        IntValue(int.Value)
    elif Int64.TryParse(value, int64) = true then
        Int64Value(int64.Value)
    elif Decimal.TryParse(value, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, decimal) = true then
        DecimalValue(decimal.Value)
    elif Boolean.TryParse(value, bool) = true then
        BoolValue(bool.Value)
    elif DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, date) = true then
        DateValue(date.Value)
    else
        StringValue(value)

/// set value of property for given ExpandoObject
let setValue expand name value = 
    (expand :> IDictionary<string,obj>).Add(name.ToString(), parseValue value)

/// add 'attributes' dictionary to ExpandoObject if not present already
/// and add parsed value there
let setAttribute expand name value : unit =
    let expandDict = (expand :> IDictionary<string,obj>)
    if not <| expandDict.ContainsKey("attributes") then
        expandDict.Add("attributes", ExpandoObject())
                            
    (expandDict.["attributes"] :?> IDictionary<string,obj>).Add(name.ToString(), parseValue value)

/// create ExpandoObject from XElement
/// support rowset and rows collections as list<obj>
let rec createExpando (node:XElement) =
    let expando = ExpandoObject()
    for e in node.Elements() do
        if e.Parent.Name.ToString().ToLower() <> "rowset" then
            let name = if not <| (e.Attribute(xn "name") = null) then
                            e.Attribute(xn "name").Value
                       else
                            e.Name.ToString()

            if e.HasElements then
                (expando :> IDictionary<string,obj>).Add(name, createExpando e)
            else
                setValue expando name e.Value
        else
            let expandDict = (expando :> IDictionary<string,obj>)
            if not <| expandDict.ContainsKey("rows") then
                expandDict.Add("rows", List<obj>())
                            
            (expandDict.["rows"] :?> List<obj>).Add(createExpando e)

    for atr in node.Attributes() do
        setAttribute expando atr.Name atr.Value

    expando

/// create XmlEveResponse from xml response from
/// Eve Api server
let createXmlObject xml = 
    use sr = new StringReader(xml)
    let xd = XDocument.Load(sr)
#if DEBUG
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
#endif
    let eveapi = xd.Element(xn "eveapi")
    let result = { Result = Some(createExpando <| eveapi.Element(xn "result"));
        Version = eveapi.Attribute(xn "version").Value;
        CurrentTime = DateTime.Parse(eveapi.Element(xn "currentTime").Value);
        CachedUntil = DateTime.Parse(eveapi.Element(xn "cachedUntil").Value);
        Error = None }
#if DEBUG
    "Parsing - " + stopwatch.Elapsed.Milliseconds.ToString() + "ms" |> Console.WriteLine |> ignore
#endif
    result

