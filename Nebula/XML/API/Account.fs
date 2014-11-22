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
    open Records

    type AccountStatusResponse = XmlProvider<"""<result><paidUntil>2011-01-01 00:00:00</paidUntil>
        <createDate>2004-01-01 00:00:00</createDate>
        <logonCount>9999</logonCount>
        <logonMinutes>9999</logonMinutes></result>""">

    type APIKeyInfoResult = XmlProvider<"""<result>
        <key accessMask="8388608" type="Character" expires="2011-09-11 00:00:00">
          <rowset name="characters" key="characterID" columns="characterID,characterName,corporationID,corporationName,allianceID,allianceName,factionID,factionName">
            <row characterID="126891489" characterName="Dragonaire" corporationID="643668601" corporationName="Here there be Dragons" allianceID="0" allianceName="" factionID="0" factionName=""/>
            <row characterID="587971565" characterName="Dragon Run1" corporationID="643668601" corporationName="Here there be Dragons" allianceID="0" allianceName="" factionID="0" factionName=""/>
          </rowset>
        </key>
      </result>
      <result>
        <key accessMask="8388608" type="Character" expires="">
          <rowset name="characters" key="characterID" columns="characterID,characterName,corporationID,corporationName,allianceID,allianceName,factionID,factionName">
            <row characterID="126891489" characterName="Dragonaire" corporationID="643668601" corporationName="Here there be Dragons" allianceID="0" allianceName="" factionID="0" factionName=""/>
            <row characterID="587971565" characterName="Dragon Run1" corporationID="643668601" corporationName="Here there be Dragons" allianceID="0" allianceName="" factionID="0" factionName=""/>
          </rowset>
        </key>
      </result>""", SampleIsList=true>

    type CharactersResult = XmlProvider<"""<result>
    <rowset name="characters" key="characterID" columns="name,characterID,corporationName,corporationID,allianceID,allianceName,factionID,factionName">
      <row name="Alexis Prey" characterID="1365215823" corporationName="Puppies To the Rescue" corporationID="238510404" allianceID="9999" allianceName="Eve-ID Puppies Inc." factionID="666" factionName="Eve-ID Faction"/>
      <row name="Alexis Prey" characterID="1365215823" corporationName="Puppies To the Rescue" corporationID="238510404" allianceID="9999" allianceName="Eve-ID Puppies Inc." factionID="666" factionName="Eve-ID Faction"/>
    </rowset>
  </result>""">

    let AccountStatus xmlResult =
        let data = AccountStatusResponse.Parse(xmlResult)
        { PaidUntil = data.PaidUntil; CreateDate = data.CreateDate; LogonCount = data.LogonCount; LogonMinutes = data.LogonMinutes }

    let APIKeyInfo xmlResult =
        let data = APIKeyInfoResult.Parse(xmlResult)
        
        let rows = data.Key.Rowset.Rows 
                    |> Seq.map (fun x -> { CharacterId = x.CharacterId; CharacterName = x.CharacterName; CorporationId = x.CorporationId; CorporationName = x.CorporationName;
                               AllianceId = x.AllianceId; AllianceName = x.AllianceName; FactionId = x.FactionId; FactionName = x.FactionName })
                    |> List.ofSeq

        { AccessMask = data.Key.AccessMask; Type = data.Key.Type; Expires = data.Key.Expires; Rows = rows }

    let Characters xmlResult =
        let data = CharactersResult.Parse(xmlResult)
        data.Rowset.Rows
        |> Seq.map (fun x -> new Character(x.Name, x.CharacterId, x.CorporationId, x.CorporationName, x.AllianceId, x.AllianceName, x.FactionId, x.FactionName))
        |> List.ofSeq