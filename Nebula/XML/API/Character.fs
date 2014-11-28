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
        
