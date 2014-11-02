namespace Nebula.XML.API.Account

module Records = 

    open System

    let internal genericToString (x:obj) = 
        let t = x.GetType()
        (t.GetProperties() |> Array.fold (fun acc prop -> acc + prop.Name + "=" + string(prop.GetValue(x, null)) + ";") "").TrimEnd(';')

    type AccountStatus = { PaidUntil:DateTime; CreateDate:DateTime; LogonCount:int; LogonMinutes:int }

    type APIKeyInfoRow = 
        { CharacterId:int; CharacterName:string; CorporationId:int; CorporationName:string;
          AllianceId:int; AllianceName:string; FactionId:int; FactionName:string }
        override x.ToString() = 
            genericToString x

    type APIKeyInfo = 
        { AccessMask:int; ``Type``:string; Expires:DateTime option; Rows:APIKeyInfoRow list } 
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