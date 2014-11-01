module Nebula.API.Account

open System
open System.Xml.Linq
open System.Net
open FSharp.Data
open FSharpx.Collections

type AccountStatusResponse = XmlProvider<"""<result><paidUntil>2011-01-01 00:00:00</paidUntil>
    <createDate>2004-01-01 00:00:00</createDate>
    <logonCount>9999</logonCount>
    <logonMinutes>9999</logonMinutes></result>""">

type AccountStatus(paidUntil:DateTime, createDate:DateTime, logonCount:int, logonMinutes:int) =
    member x.PaidUntil = paidUntil
    member x.CreateDate = createDate
    member x.LogonCount = logonCount
    member x.LogonMinutes = logonMinutes

let AccountStatus xmlResult =
    let data = AccountStatusResponse.Parse(xmlResult)
    new AccountStatus(data.PaidUntil, data.CreateDate, data.LogonCount, data.LogonMinutes)