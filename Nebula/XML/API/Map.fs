namespace Nebula.XML.API.Map

module Records = 

    open System
    open Nebula.XML.API.Shared

    type FactionalWarfareSystem =
        { SolarSystemId:int; SolarSystemName:string; OccupyingFactionId:int; OccupyingFactionName:string; Contested:bool }
        override x.ToString() =
            genericToString x

    type JumpsRow =
        { SolarSystemId: int; ShipJumps:int }
        override x.ToString() =
            genericToString x
    type Jumps = 
        { DataTime: DateTime; Rows: JumpsRow list }
        override x.ToString() =
            genericToString x

    type KillsRow = 
        { SolarSystemId:int; ShipKills:int; FactionKills:int; PodKills:int }
        override x.ToString() =
            genericToString x
    type Kills = 
        { DataTime:DateTime; Rows: KillsRow list }
        override x.ToString() =
            genericToString x

    type SovereigntyRow = 
        { SolarSystemId:int; SolarSystemName:string; AllianceId:int; FactionId:int; CorporationId:int }
        override x.ToString() =
            genericToString x
    type Sovereignty = 
        { DataTime:DateTime; Rows: SovereigntyRow list }
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

    let FacWarSystems (xmlResult:XmlEveResponse) =
        (fun result ->
            result?rowset?rows
            |> Seq.map (fun x -> 
                let xa = x?attributes
                { SolarSystemId = mi xa?solarSystemID; 
                    SolarSystemName = ms xa?solarSystemName; 
                    OccupyingFactionId = mi xa?occupyingFactionID;
                    OccupyingFactionName = ms xa?occupyingFactionName; 
                    Contested = mb xa?contested })
            |> List.ofSeq)
        |> handleResult xmlResult

    let Jumps (xmlResult:XmlEveResponse) : Jumps =
        (fun result ->
            let rows = 
                result?rowset?rows
                |> Seq.map (fun x -> 
                    let xa = x?attributes
                    { SolarSystemId = mi xa?solarSystemID; 
                        ShipJumps = mi xa?shipJumps })
                |> List.ofSeq

            { Jumps.DataTime = mdt result?dataTime; 
                Rows = rows })
        |> handleResult xmlResult

    let Kills (xmlResult:XmlEveResponse) : Kills =
        (fun result ->
            let rows = 
                result?rowset?rows
                |> Seq.map (fun x -> 
                    let xa = x?attributes
                    { SolarSystemId = mi xa?solarSystemID; 
                        ShipKills = mi xa?shipKills; 
                        FactionKills = mi xa?factionKills; 
                        PodKills = mi xa?podKills})
                |> List.ofSeq

            { Kills.DataTime = mdt result?dataTime; 
                Rows = rows })
        |> handleResult xmlResult

    let Sovereignty (xmlResult:XmlEveResponse) : Sovereignty =
        (fun result ->
            let rows = 
                result?rowset?rows
                |> Seq.map (fun x -> 
                    let xa = x?attributes
                    { SolarSystemId = mi xa?solarSystemID; 
                        SolarSystemName = ms xa?solarSystemName; 
                        AllianceId = mi xa?allianceID;
                        FactionId = mi xa?factionID;
                        CorporationId = mi xa?corporationID })
                |> List.ofSeq

            { Sovereignty.DataTime = mdt result?dataTime; 
                Rows = rows })
        |> handleResult xmlResult