namespace Nebula.XML.API.Character

module Records = 

    open System
    open Nebula.XML.API.Shared

    type AccountBalance = 
        { AccountId:int; AccountKey:int; Balance:decimal }
        override x.ToString() = 
            genericToString x

    type Asset = 
        { ItemId:int64; LocationId:int64 option; TypeId:int; Quantity:int; Flag:int; 
          Singleton:bool; RawQuantity:int option; Content:Asset list option;
          Item:Nebula.SDE.Items.InvType option }
         override x.ToString() = 
            genericToString x

    type Blueprint = 
        { ItemId:int64; LocationId:int64; TypeId:int; TypeName:string;
          FlagId:int; Quantity: int; TimeEfficiency: int; MaterialEfficiency: int;
          Runs: int }
          override x.ToString() =
            genericToString x

module internal Calls =

    open System
    open FSharp.Data
    open Records
    open System.Xml.Linq

    type AccountBalanceResult = XmlProvider<"""<result>
    <rowset name="accounts" key="accountID" columns="accountID,accountKey,balance">
      <row accountID="4807144" accountKey="1000" balance="209127823.31" />
    </rowset>
  </result>""">

    type BlueprintResult = XmlProvider<"""<result>
    <rowset name="blueprints" key="itemID" 
            columns="itemID,locationID,typeID,typeName,flagID,quantity,timeEfficiency,materialEfficiency,runs">
      <row itemID="1000000012172" locationID="60014506" typeID="11872" typeName="R.A.M.- Ammunition Tech Blueprint" 
           flagID="4" quantity="-1" timeEfficiency="0" materialEfficiency="0" runs="-1" />
      <row itemID="1000000012173" locationID="60014506" typeID="11872" typeName="R.A.M.- Ammunition Tech Blueprint" 
           flagID="4" quantity="-1" timeEfficiency="20" materialEfficiency="10" runs="-1" />
      <row itemID="1000000012175" locationID="60014506" typeID="808" typeName="Mjolnir Heavy Missile Blueprint" 
           flagID="4" quantity="-2" timeEfficiency="0" materialEfficiency="10" runs="490" />
      <row itemID="1000000012176" locationID="1015338129650" typeID="10681" typeName="125mm Railgun II Blueprint"
           flagID="116" quantity="-2" timeEfficiency="2" materialEfficiency="3" runs="12"/>
      <row itemID="1000000012177" locationID="1015338129650" typeID="972" typeName="Thorax Blueprint"
           flagID="117" quantity="-2" timeEfficiency="20" materialEfficiency="10" runs="1"/>
    </rowset>
  </result>""">

    let AccountBalance xmlResult =

        let data = AccountBalanceResult.Parse(xmlResult)
        let row = data.Rowset.Row
        { AccountId = row.AccountId; AccountKey = row.AccountKey; Balance = row.Balance }

    let AssetList xmlResult =
        let xn = XName.op_Implicit
        let data = XElement.Parse(xmlResult).Element(xn "rowset")
        let rec loadData (data: XElement) = 
            let rows = query { for a in data.Elements(xn "row") do
                               select { ItemId = int64(a.Attribute(xn "itemID").Value);
                                        TypeId = int(a.Attribute(xn "typeID").Value);
                                        Item = Nebula.SDE.Items.getItem (int64(a.Attribute(xn "typeID").Value));
                                        RawQuantity = if a.Attribute(xn "rawQuantity") <> null then
                                                        Some(int(a.Attribute(xn "rawQuantity").Value))
                                                      else None;
                                        LocationId = if a.Attribute(xn "locationID") <> null then
                                                        Some(int64(a.Attribute(xn "locationID").Value))
                                                     else None;
                                        Quantity = if a.Attribute(xn "quantity") <> null then
                                                        int(a.Attribute(xn "quantity").Value)
                                                   else 1;
                                        Singleton = if a.Attribute(xn "singleton") <> null then
                                                       let at = a.Attribute(xn "singleton").Value 
                                                       at = "1"
                                                    else false;
                                        Flag = if a.Attribute(xn "flag") <> null then
                                                    int(a.Attribute(xn "flag").Value)
                                                else 0;
                                        Content = if a.HasElements then
                                                    a.Element(xn "rowset")
                                                    |> loadData
                                                  else None } }

            Some(List.ofSeq rows)

        loadData data
        

    let Blueprints xmlResult =
        let data = BlueprintResult.Parse(xmlResult)
        let rows = seq { for row in data.Rowset.Rows do 
                         yield { ItemId = row.ItemId;
                           LocationId = row.LocationId;
                           TypeId = row.TypeId;
                           TypeName = row.TypeName;
                           FlagId = row.FlagId;
                           Quantity = row.Quantity;
                           TimeEfficiency = row.TimeEfficiency;
                           MaterialEfficiency = row.MaterialEfficiency;
                           Runs = row.Runs }
                        }

        rows
        |> List.ofSeq



