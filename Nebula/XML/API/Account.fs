namespace Nebula.XML.API.Account

module Records = 

    open System
    open Nebula.XML.API.Shared

    type AccountStatus = 
        { PaidUntil:DateTime; CreateDate:DateTime; LogonCount:int; LogonMinutes:int }
        override x.ToString() = 
            genericToString x

    type APIKeyInfoRow = 
        { CharacterId:int; CharacterName:string; CorporationId:int; CorporationName:string;
          AllianceId:int; AllianceName:string; FactionId:int; FactionName:string }
        override x.ToString() = 
            genericToString x

    type APIKeyInfo = 
        { AccessMask:int; ``Type``:string; Expires:DateTime option; Rows:APIKeyInfoRow list } 
        override x.ToString() = 
            genericToString x

    type Character(name, characterId, corporationId, corporationName, allianceId, allianceName, factionId, factionName) = 
        let mutable api = null
        member internal x.Api
            with get() = api
            and set(value) = api <- value 

        member val Name:string = name 
        member val CharacterId:int = characterId
        member val CorporationId:int = corporationId
        member val CorporationName:string = corporationName
        member val AllianceId:int = allianceId
        member val AllianceName:string = allianceName
        member val FactionId:int = factionId
        member val FactionName:string = factionName
        override x.ToString() = 
            genericToString x

module internal Calls =

    open System
    open FSharp.Data
    open EkonBenefits.FSharp.Dynamic
    open Records
    open Nebula.ApiTypes
    open Nebula.XML.API.Shared
    open Nebula.XmlToExpando

    let AccountStatus (xmlResult:XmlEveResponse) =
        (fun result ->
            { PaidUntil = mdt result?paidUntil; 
              CreateDate = mdt result?createDate; 
              LogonCount = mi result?logonCount; 
              LogonMinutes = mi result?logonMinutes })
        |> handleResult xmlResult

    let APIKeyInfo (xmlResult:XmlEveResponse) =
        (fun result ->
            let rows = 
                result?key?rowset?rows
                |> Seq.map (fun x ->
                    let xa = x?attributes
                    { CharacterId = mi xa?characterID; 
                        CharacterName = ms xa?characterName; 
                        CorporationId = mi xa?corporationID; 
                        CorporationName = ms xa?corporationName;
                        AllianceId = mi xa?allianceID; 
                        AllianceName = ms xa?allianceName; 
                        FactionId = mi xa?factionID; 
                        FactionName = ms xa?factionName })
                |> List.ofSeq
                
            let keyAttr = result?key?attributes
            { AccessMask = mi keyAttr?accessMask; 
                Type = ms keyAttr?``type``; 
                Expires = (if ms keyAttr?expires = "" then None else Some(mdt keyAttr?expires)); 
                Rows = rows })
        |> handleResult xmlResult        

    let Characters (xmlResult:XmlEveResponse) =
        (fun result ->
            result?characters?rows
            |> Seq.map (fun x -> new Character(ms x?attributes?name, 
                                               mi x?attributes?characterID, 
                                               mi x?attributes?corporationID, 
                                               ms x?attributes?corporationName, 
                                               mi x?attributes?allianceID, 
                                               ms x?attributes?allianceName, 
                                               mi x?attributes?factionID, 
                                               ms x?attributes?factionName))
            |> List.ofSeq)
        |> handleResult xmlResult