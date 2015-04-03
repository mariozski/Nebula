module Nebula.Test.Api

open Nebula.XmlToExpando
open Xunit
open FsCheck.Xunit
open System
open System.Dynamic
open System.Collections.Generic
open EkonBenefits.FSharp.Dynamic


// ------------------------------------------
// value parser tests             
// ------------------------------------------
// api uses strictly this format for dates
[<Fact>]
let ``yyyy-MM-dd HH:mm:ss should be date``() =
    let v = parseValue "2014-01-01 00:00:00"
    Assert.IsType(typeof<DateTime>, v)
    Assert.Equal(DateTime(2014,1,1), v :?> DateTime)

// there are cases of such solar systems names
[<Fact>]
let ``MM-yyyy should be string``() =
    let date = "05-2000"
    let v = parseValue date
    Assert.IsType(typeof<string>, v)
    Assert.Equal(date, v :?> string)

[<Fact>]
let ``0.12 should be decimal``() =
    let v = parseValue "0.12"
    Assert.IsType(typeof<decimal>, v)
    Assert.Equal(0.12m, v :?> decimal)

[<Fact>]
let ``2.0 should be decimal``() =
    let v = parseValue "2.0"
    Assert.IsType(typeof<decimal>, v)
    Assert.Equal(2.0m, v :?> decimal)

[<Fact>]
let ``554 should be int``() =
    let v = parseValue "554"
    Assert.IsType(typeof<int32>, v)
    Assert.Equal(554, v :?> int32)

[<Fact>]
let ``0 should be int``() =
    let v = parseValue "0"
    Assert.IsType(typeof<int32>, v)
    Assert.Equal(0, v :?> int32)

[<Fact>]
let ``1111111111232 should be int64``() =
    let v = parseValue "1111111111232"
    Assert.IsType(typeof<int64>, v)
    Assert.Equal(1111111111232L, v :?> int64)

[<Fact>]
let ``True or False should be boolean``() =
    let t = parseValue "True"
    let f = parseValue "False"
    Assert.IsType(typeof<bool>, t)
    Assert.IsType(typeof<bool>, f)
    Assert.Equal(true, t :?> bool)
    Assert.Equal(false, f :?> bool)

[<Fact>]
let ``00-1AB should be string``() =
    let v = parseValue "00-1AB"
    Assert.IsType(typeof<string>, v)
    Assert.Equal("00-1AB", v :?> string)

[<Fact>]
let ``capitalize should make first char of string capital``() =
    let a = capitalize "one two"
    Assert.Equal("One two", a)

// --------------------------------
// expando operations tests
// --------------------------------
[<Fact>]
let ``setValue should set property of expando with proper value``() =
    let e = ExpandoObject()
    setValue e "test" "0.12"
    let containsKey = (e :> IDictionary<string, obj>).ContainsKey("test")
    Assert.True(containsKey)
    Assert.Equal(0.12m, e?test)

[<Fact>]
let ``setAttribute should set property of expando with attributes dict and proper value``() =
    let e = ExpandoObject()
    setAttribute e "test" "0.12"
    Assert.True((e :> IDictionary<string, obj>).ContainsKey("attributes"))
    Assert.True((e?attributes :> IDictionary<string, obj>).ContainsKey("test"))
    Assert.Equal(0.12m, e?attributes?test)