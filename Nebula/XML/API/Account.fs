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
    open ApiTypes
    open Nebula.XML.API.Shared

    let AccountStatus (xmlResult:XmlEveResponse) =
        (fun result ->
            let a = result?paidUntil
            let b= result?createDate
            let c= result?logonCount
            let d = result?logonMinutes
            { PaidUntil = result?paidUntil; 
              CreateDate = result?createDate; 
              LogonCount = result?logonCount; 
              LogonMinutes = result?logonMinutes })
        |> handleResult xmlResult

    let APIKeyInfo (xmlResult:XmlEveResponse) =
        (fun result ->
            let rows = 
                result?key?rowset?rows
                |> Seq.map (fun x ->
                    let xa = x?attributes
                    { CharacterId = xa?characterID; 
                        CharacterName = xa?characterName; 
                        CorporationId = xa?corporationID; 
                        CorporationName = xa?corporationName;
                        AllianceId = xa?allianceID; 
                        AllianceName = xa?allianceName; 
                        FactionId = xa?factionID; 
                        FactionName = xa?factionName })
                |> List.ofSeq
                
            let keyAttr = result?key?attributes
            { AccessMask = keyAttr?accessMask; 
                Type = keyAttr?``type``; 
                Expires = (if keyAttr?expires = "" then None else Some(keyAttr?expires)); 
                Rows = rows })
        |> handleResult xmlResult        

    let Characters (xmlResult:XmlEveResponse) =
        (fun result ->
            result?rowset?rows
            |> Seq.map (fun x -> new Character(x?attributes?name, 
                                               x?attributes?characterID, 
                                               x?attributes?corporationID, 
                                               x?attributes?corporationName, 
                                               x?attributes?allianceID, 
                                               x?attributes?allianceName, 
                                               x?attributes?factionID, 
                                               x?attributes?factionName))
            |> List.ofSeq)
        |> handleResult xmlResult