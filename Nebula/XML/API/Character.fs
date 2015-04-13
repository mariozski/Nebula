namespace Nebula.XML.API.Character

module Records = 

    open System
    open Nebula.XML.API.Shared

    type AccountBalance = 
        { AccountId:int; AccountKey:int; Balance:decimal }
        override x.ToString() = 
            genericToString x

    type Asset = 
        { ItemId:int64; LocationId:LocationId option; TypeId:TypeId; Quantity:int; Flag:int; 
          Singleton: bool; RawQuantity:int option; Content:Asset list option;
          Item:Nebula.SDE.Items.InvType option }
         override x.ToString() = 
            genericToString x

    type Blueprint = 
        { ItemId:int64; LocationId:LocationId; TypeId:TypeId; TypeName:string;
          FlagId:int; Quantity: int; TimeEfficiency: int; MaterialEfficiency: int;
          Runs: int }
          override x.ToString() =
            genericToString x
    
    

    type Race =
    | Minmatar = 0
    | Amarr = 1
    | Gallente = 2
    | Caldari = 3

    type Gender =
    | Male = 0
    | Female = 1

    type Attributes = 
        {   Intelligence:int
            Memory:int
            Charisma:int
            Perception:int
            Willpower:int }
        override x.ToString() =
            genericToString x

    type JumpClone = 
       { JumpCloneId:int64
         TypeId:TypeId
         LocationId:LocationId
         CloneName:string }
       override x.ToString() =
            genericToString x

    type JumpCloneImplant = 
        {   JumpCloneId:int64
            TypeId:TypeId
            TypeName:string }
        override x.ToString() =
            genericToString x

    type Implant = 
        {   TypeId:TypeId
            TypeName:string }
        override x.ToString() =
            genericToString x

    type Skill = 
        {   TypeId:TypeId
            SkillPoints:int
            Level:int
            Published:int }
        override x.ToString() =
            genericToString x

    type CharacterSheet = 
        {   CharacterId:int64
            Name:string
            HomeStationId:int
            DateOfBirth:DateTime
            Race:Race
            BloodLine:string
            Ancestry:string
            Gender:Gender
            CorporationName:string
            CorporationId:int64
            AllianceName:string
            AllianceId:int64
            FactionName:string
            FactionId:int64
            FreeSkillPoints:int
            FreeRespecs:int
            CloneJumpDate:DateTime
            LastRespecDate:DateTime
            LastTimedRespec:DateTime
            JumpActiation:DateTime
            JumpFatigue:DateTime
            JumpLastUpdate:DateTime
            Balance:decimal
            Attributes:Attributes
            JumpClones:JumpClone list
            JumpCloneImplants:JumpCloneImplant list
            Implants:Implant list
            Skills:Skill list
            Certificates:int list }
        override x.ToString() =
            genericToString x

module internal Calls =

    open System
    open FSharp.Data
    open FSharpx.Collections
    open EkonBenefits.FSharp.Dynamic
    open Records
    open System.Xml.Linq
    open Nebula.ApiTypes
    open Nebula.XML.API.Shared
    open System.Collections.Generic
    open System.Dynamic
    open Nebula.XmlToExpando

    let AccountBalance xmlResult =
        (fun result ->
            let row =
                result?rowset?rows
                |> List.ofSeq
                |> List.head
            
            let xa = row?attributes
            { AccountId = xa?accountID; 
                AccountKey = xa?accountKey; 
                Balance = xa?balance })
        |> handleResult xmlResult

    let AssetList xmlResult =
        (fun result ->
            let assetsRows = result?assets?rows
            let rec readAssets (assets:seq<obj>) = 
                assets
                |> List.ofSeq
                |> List.map 
                    (fun asset ->
                        let a = asset?attributes
                        let ad = a :> IDictionary<string,obj>
                        { ItemId = mi64 a?itemID
                          LocationId = if ad.ContainsKey("locationID") then Some(mi64 a?locationID) else None
                          TypeId = mi a?typeID
                          Quantity = mi a?quantity
                          Flag = mi a?flag
                          Singleton = mb a?singleton
                          RawQuantity = if ad.ContainsKey("rawQuantity") then Some(mi a?rawQuantity) else None
                          Content = if (asset :?> IDictionary<string,obj>).ContainsKey("contents") then
                                        Some(readAssets asset?contents?rows)
                                    else
                                        None
                          Item = None }
                    )

            readAssets assetsRows)
        |> handleResult xmlResult       

    let Blueprints xmlResult =
        (fun result ->
            let rowset = result?rowset :> IDictionary<string, obj>
            if not <| rowset.ContainsKey("rows") then 
                List.empty<Blueprint>
            else
                [ 
                    for row in rowset?rows do
                    let a = row?attributes
                    yield {
                        ItemId = mi64 a?itemID
                        LocationId = mi64 a?locationID
                        TypeId = mi a?typeID
                        TypeName = ms a?typeName
                        FlagId = mi a?flagID
                        Quantity = mi a?quantity
                        TimeEfficiency = mi a?timeEfficiency
                        MaterialEfficiency = mi a?materialEfficiency
                        Runs = mi a?runs
                    }
                ])
        |> handleResult xmlResult

    let CharacterSheet xmlResult =
        (fun result -> 
        {
            CharacterId = mi64 result?characterID
            Name = ms result?name
            HomeStationId = mi result?homeStationID
            DateOfBirth = mdt result?DoB
            Race = Enum.Parse(typeof<Race>, ms result?race) :?> Race
            BloodLine = ms result?bloodLine
            Ancestry = ms result?ancestry
            Gender = Enum.Parse(typeof<Gender>, ms result?gender) :?> Gender
            CorporationName = ms result?corporationName
            CorporationId = mi64 result?corporationID
            AllianceName = ms result?allianceName
            AllianceId = mi64 result?allianceID
            FactionName = ms result?factionName
            FactionId = mi64 result?factionID
            FreeSkillPoints = mi result?freeSkillPoints
            FreeRespecs = mi result?freeRespecs
            CloneJumpDate = mdt result?cloneJumpDate
            LastRespecDate = mdt result?lastRespecDate
            LastTimedRespec = mdt result?lastTimedRespec
            JumpActiation = mdt result?jumpActivation
            JumpFatigue = mdt result?jumpFatigue
            JumpLastUpdate = mdt result?jumpLastUpdate
            Balance = md result?balance
            Attributes = {  Intelligence = mi result?attributes?intelligence
                            Memory = mi result?attributes?memory
                            Charisma = mi result?attributes?charisma
                            Perception = mi result?attributes?perception
                            Willpower = mi result?attributes?willpower }
            JumpClones = List.empty<JumpClone>
            JumpCloneImplants = List.empty<JumpCloneImplant>
            Implants = List.empty<Implant>
            Skills = List.empty<Skill>
            Certificates = List.empty<int>
        })
        |> handleResult xmlResult



