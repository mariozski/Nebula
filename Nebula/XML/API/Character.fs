namespace Nebula.XML.API.Character

module Records = 

    open System
    open Nebula.XML.API.Shared

    type AccountBalance = 
        { AccountId:int; AccountKey:int; Balance:decimal }
        override x.ToString() = 
            genericToString x

module internal Calls =

    open System
    open FSharp.Data
    open Records

    type AccountBalanceResult = XmlProvider<"""<result>
    <rowset name="accounts" key="accountID" columns="accountID,accountKey,balance">
      <row accountID="4807144" accountKey="1000" balance="209127823.31" />
    </rowset>
  </result>""">

    let AccountBalance xmlResult =
        let data = AccountBalanceResult.Parse(xmlResult)
        let row = data.Rowset.Row
        { AccountId = row.AccountId; AccountKey = row.AccountKey; Balance = row.Balance }
        
