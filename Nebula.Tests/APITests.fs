module Nebula.Test.Api

open Nebula.ApiTypes
open Nebula.XmlToExpando
open Xunit
open FsCheck.Xunit
open System
open System.Collections.Generic

// ------------------------------------------
// value parser tests             
// ------------------------------------------
// api uses strictly this format for dates
[<Fact>]
let ``yyyy-MM-dd HH:mm:ss should be date``() =
    let v = parseValue "2014-01-01 00:00:00"
    Assert.Equal(
        DateTime(2014,1,1), 
        match v with
        | DateValue(x) -> x
        | _ -> DateTime.MinValue)

// there are cases of such solar systems names
[<Fact>]
let ``MM-yyyy should be string``() =
    let date = "05-2000"
    let v = parseValue date
    Assert.Equal(
        date, 
        match v with 
        | StringValue(x) -> x
        | _ -> String.Empty)

[<Fact>]
let ``0.12 should be decimal``() =
    let v = parseValue "0.12"
    Assert.Equal(
        0.12m, 
        match v with
        | DecimalValue(x) -> x
        | _ -> 0m)

[<Fact>]
let ``2.0 should be decimal``() =
    let v = parseValue "2.0"
    Assert.Equal(
        2.0m, 
        match v with
        | DecimalValue x -> x
        | _ -> 0m)

[<Fact>]
let ``554 should be int``() =
    let v = parseValue "554"
    Assert.Equal(554, 
        match v with
        | IntValue x -> x
        | _ -> 0)

[<Fact>]
let ``0 should be int``() =
    let v = parseValue "0"
    Assert.Equal(0, 
        match v with
        | IntValue x -> x
        | _ -> -1)

[<Fact>]
let ``1111111111232 should be int64``() =
    let v = parseValue "1111111111232"
    Assert.Equal(1111111111232L, 
        match v with
        | Int64Value x -> x
        | _ -> 0L)

[<Fact>]
let ``True or False should be boolean``() =
    let t = parseValue "True"
    let f = parseValue "False"
    Assert.Equal(true, 
        match t with
        | BoolValue x -> x
        | _ -> false)
    Assert.Equal(false, 
        match f with
        | BoolValue x -> x
        | _ -> true)

[<Fact>]
let ``00-1AB should be string``() =
    let v = parseValue "00-1AB"
    Assert.Equal("00-1AB", 
        match v with
        | StringValue x -> x
        | _ -> String.Empty)

[<Fact>]
let ``capitalize should make first char of string capital``() =
    let a = capitalize "one two"
    Assert.Equal("One two", a)