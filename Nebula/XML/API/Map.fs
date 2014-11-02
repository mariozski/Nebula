namespace Nebula.XML.API.Map

module Records = 

    open System
    open Nebula.XML.API.Shared

    type FactionalWarfareSystem =
        { SolarSystemId:int; SolarSystemName:string; OccupyingFactionId:int; OccupyingFactionName:string option; Contested:bool }
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

module internal Calls =
    open System
    open FSharp.Data
    open Records

    type FacWarSystemsResult = XmlProvider<"""<result>
       <rowset name="solarSystems" key="solarSystemID" columns="solarSystemID,solarSystemName,occupyingFactionID,occupyingFactionName,contested">
         <row solarSystemID="30002056" solarSystemName="Resbroko" occupyingFactionID="0" occupyingFactionName="" contested="True"/>
         <row solarSystemID="30002057" solarSystemName="Hadozeko" occupyingFactionID="0" occupyingFactionName="" contested="False"/>
         <row solarSystemID="30003068" solarSystemName="Kourmonen" occupyingFactionID="500002" occupyingFactionName="Minmatar Republic" contested="False"/>
         <row solarSystemID="30003069" solarSystemName="Kamela" occupyingFactionID="500002" occupyingFactionName="Minmatar Republic" contested="True"/>
      </rowset></result>""">

    type JumpsResult = XmlProvider<"""<result>
    <rowset name="solarSystems" key="solarSystemID" columns="solarSystemID,shipJumps">
      <row solarSystemID="30001984" shipJumps="10" />
      <row solarSystemID="30001984" shipJumps="10" />
    </rowset>
    <dataTime>2007-12-12 11:50:38</dataTime></result>""">

    let FacWarSystems xmlResult =
        let data = FacWarSystemsResult.Parse xmlResult
        data.Rowset.Rows
        |> Seq.map (fun x -> { SolarSystemId = x.SolarSystemId; SolarSystemName = x.SolarSystemName; OccupyingFactionId = x.OccupyingFactionId;
                               OccupyingFactionName = x.OccupyingFactionName; Contested = x.Contested })
        |> List.ofSeq

    let Jumps xmlResult =
        let data = JumpsResult.Parse xmlResult
        { DataTime = data.DataTime; Rows = data.Rowset.Rows
                                    |> Seq.map (fun x -> { SolarSystemId = x.SolarSystemId; ShipJumps = x.ShipJumps })
                                    |> List.ofSeq }