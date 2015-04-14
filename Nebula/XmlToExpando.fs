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

let mi x =
    match x with
    | IntValue v -> v
    | _          -> failwithf "Cannot match %A to int" x

let mi64 x =
    match x with
    | Int64Value v  -> v
    | IntValue v    -> int64 v
    | _ -> failwithf "Cannot match %A to int64" x

let ms x =
    match x with
    | StringValue v -> v
    | _ as v        -> v.ToString()

let md x =
    match x with
    | DecimalValue v  -> v
    | IntValue v      -> decimal v
    | Int64Value v    -> decimal v
    | _               -> failwithf "Cannot match %A to decimal" x

let mb x =
    match x with
    | BoolValue v   -> v
    | IntValue v    -> 
        match v with
        | 0 -> false
        | _ -> true
    | StringValue v ->
        match v with
        | "True" | "true"   -> true
        | "False" | "false" -> false
        | _                 -> failwithf "Cannot match %A to bool" v
    | _ -> failwithf "Cannot match %A to bool" x

let mdt x =
    match x with 
    | DateValue v   -> v
    | _             -> failwithf "Cannot match %A to date" x

/// Parse value from XML based on format
let parseValue value : Object = 
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

/// add 'attributes' dictionary to ExpandoObject if not present already
/// and add parsed value there
let setAttribute expand name value : unit =
    let expandDict = (expand :> IDictionary<string,obj>)
    if not <| expandDict.ContainsKey("attributes") then
        expandDict.Add("attributes", ExpandoObject())
                            
    (expandDict.["attributes"] :?> IDictionary<string,obj>).Add(name.ToString(), Some <| parseValue value)

/// create ExpandoObject from XElement
/// support rowset and rows collections as list<obj>
let rec createContent (node:XElement) : Object =
    let elements = 
        [ for e in node.Elements() do 
            match e.Parent.Name.ToString().ToLower() with
            | "rowset" -> yield ("rows", createContent e)
            | _ ->
                let name = match e.Attribute(xn "name") with
                           | null -> e.Name.ToString()
                           | v -> e.Attribute(xn "name").Value

                match e.HasElements with
                | true -> yield (name, createContent e)
                | false -> yield (name, parseValue e.Value) ]
        
    let attributes =
        ("attr", [ for atr in node.Attributes() do  
                        yield (atr.Name.ToString(), parseValue atr.Value) ]
                   |> Map.ofList
                   |> MapValue)

    attributes :: elements
    |> Seq.groupBy (fun x -> fst x)
    |> Seq.map (fun x -> 
        (fst x, match snd x |> Seq.map (fun y -> snd y) |> List.ofSeq with
                | head::[] -> if fst x = "rows" then ListValue [ head ] else head
                | array -> ListValue array))
    |> Map.ofSeq
    |> MapValue

/// create XmlEveResponse from xml response from
/// Eve Api server
let createXmlObject xml = 
    use sr = new StringReader(xml)
    let xd = XDocument.Load(sr)
#if DEBUG
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
#endif
    let eveapi = xd.Element(xn "eveapi")
    let result = { Result = Some(createContent <| eveapi.Element(xn "result"));
        Version = eveapi.Attribute(xn "version").Value;
        CurrentTime = DateTime.Parse(eveapi.Element(xn "currentTime").Value);
        CachedUntil = DateTime.Parse(eveapi.Element(xn "cachedUntil").Value);
        Error = None }
#if DEBUG
    "Parsing - " + stopwatch.Elapsed.Milliseconds.ToString() + "ms" |> Console.WriteLine |> ignore
#endif
    result

