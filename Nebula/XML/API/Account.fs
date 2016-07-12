namespace Nebula.XML.API.Account

module Records = 

    open System
    open Nebula.XML.API.Shared

    type AccountStatus = 
        { PaidUntil:DateTime; CreateDate:DateTime; LogonCount:int; LogonMinutes:int }
        override x.ToString() = 
            genericToString x

    type APIKeyInfoRow = 
        { CharacterId:int64; CharacterName:string; CorporationId:int64; CorporationName:string;
          AllianceId:int64; AllianceName:string; FactionId:int64; FactionName:string }
        override x.ToString() = 
            genericToString x

    type APIKeyInfo = 
        { AccessMask:int64; ``Type``:string; Expires:DateTime option; Rows:APIKeyInfoRow list } 
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
                result?key?characters?>"rows"
                |> Seq.map (fun x ->
                    let xa = x?attr
                    { CharacterId = mi64 xa?characterID; 
                        CharacterName = ms xa?characterName; 
                        CorporationId = mi64 xa?corporationID; 
                        CorporationName = ms xa?corporationName;
                        AllianceId = mi64 xa?allianceID; 
                        AllianceName = ms xa?allianceName; 
                        FactionId = mi64 xa?factionID; 
                        FactionName = ms xa?factionName })
                |> List.ofSeq
                
            let keyAttr = result?key?attr
            { AccessMask = mi64 keyAttr?accessMask; 
                Type = ms keyAttr?``type``; 
                Expires = (if ms keyAttr?expires = "" then None else Some(mdt keyAttr?expires)); 
                Rows = rows })
        |> handleResult xmlResult        

    let Characters (xmlResult:XmlEveResponse) =
        (fun result ->
            result?characters?>"rows"
            |> Seq.map (fun x -> 
                            new Character(ms x?attr?name, 
                                            mi x?attr?characterID, 
                                            mi x?attr?corporationID, 
                                            ms x?attr?corporationName, 
                                            mi x?attr?allianceID, 
                                            ms x?attr?allianceName, 
                                            mi x?attr?factionID, 
                                            ms x?attr?factionName))
            |> List.ofSeq)
        |> handleResult xmlResult