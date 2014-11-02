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

    type KillsResult = XmlProvider<"""<result>
    <rowset name="solarSystems" key="solarSystemID" columns="solarSystemID,shipKills,factionKills,podKills">
      <row solarSystemID="30001343" shipKills="0" factionKills="17" podKills="0" />
      <row solarSystemID="30002671" shipKills="0" factionKills="340" podKills="0" />
      <row solarSystemID="30005327" shipKills="0" factionKills="21" podKills="0" />
      <row solarSystemID="30002410" shipKills="0" factionKills="3" podKills="0" />
      <row solarSystemID="30001082" shipKills="0" factionKills="3" podKills="0" />
      <row solarSystemID="30001105" shipKills="0" factionKills="6" podKills="0" />
      <row solarSystemID="30001937" shipKills="0" factionKills="14" podKills="0" />
      <row solarSystemID="30003560" shipKills="0" factionKills="3" podKills="0" />
      <row solarSystemID="30002478" shipKills="3" factionKills="15" podKills="2" />
      <row solarSystemID="30004101" shipKills="0" factionKills="22" podKills="0" />
    </rowset>
    <dataTime>2007-12-16 10:57:53</dataTime>
  </result>""">

    type SovereigntyResult = XmlProvider<"""<result>
    <rowset name="solarSystems" key="solarSystemID" columns="solarSystemID,allianceID,factionID,solarSystemName,corporationID">
      <row solarSystemID="30023410" allianceID="0" factionID="500002" solarSystemName="Embod" corporationID="0"/>
      <row solarSystemID="30001597" allianceID="1028876240" factionID="0" solarSystemName="M-NP5O" corporationID="421957727" />
      <row solarSystemID="30001815" allianceID="389924442" factionID="0" solarSystemName="4AZV-W" corporationID="123456789"/>
      <row solarSystemID="30001816" allianceID="0" factionID="0" solarSystemName="UNV-3J" corporationID="123456789"/>
      <row solarSystemID="30000479" allianceID="0" factionID="0" solarSystemName="SLVP-D" corporationID="123456789"/>
      <row solarSystemID="30000480" allianceID="824518128" factionID="0" solarSystemName="0-G8NO" corporationID="123456789"/>
    </rowset>
    <dataTime>2009-12-23 05:16:38</dataTime>
  </result>""">

    let FacWarSystems xmlResult =
        let data = FacWarSystemsResult.Parse xmlResult
        data.Rowset.Rows
        |> Seq.map (fun x -> { SolarSystemId = x.SolarSystemId; SolarSystemName = x.SolarSystemName; OccupyingFactionId = x.OccupyingFactionId;
                               OccupyingFactionName = x.OccupyingFactionName; Contested = x.Contested })
        |> List.ofSeq

    let Jumps xmlResult : Jumps =
        let data = JumpsResult.Parse xmlResult
        { DataTime = data.DataTime; Rows = data.Rowset.Rows
                                    |> Seq.map (fun x -> { SolarSystemId = x.SolarSystemId; ShipJumps = x.ShipJumps })
                                    |> List.ofSeq }

    let Kills xmlResult : Kills =
        let data = KillsResult.Parse xmlResult
        { DataTime = data.DataTime; Rows = data.Rowset.Rows
                                    |> Seq.map (fun x -> { SolarSystemId = x.SolarSystemId; ShipKills = x.ShipKills; FactionKills = x.FactionKills; PodKills = x.PodKills})
                                    |> List.ofSeq }

    let Sovereignty xmlResult : Sovereignty =
        let data = SovereigntyResult.Parse xmlResult
        { DataTime = data.DataTime; Rows = data.Rowset.Rows
                                    |> Seq.map (fun x -> { SolarSystemId = x.SolarSystemId; SolarSystemName = x.SolarSystemName; AllianceId = x.AllianceId;
                                                           FactionId = x.FactionId; CorporationId = x.CorporationId })
                                    |> List.ofSeq }