namespace Nebula.XML.API.Character

module Records = 
    open System
    open Nebula.XML.API.Shared
    
    type AccountBalance = 
        { AccountId : int
          AccountKey : int
          Balance : decimal }
        override x.ToString() = genericToString x
    
    type Asset = 
        { ItemId : int64
          LocationId : LocationId option
          TypeId : TypeId
          Quantity : int
          Flag : int
          Singleton : bool
          RawQuantity : int option
          Content : Asset list
          Item : Nebula.SDE.Items.InvType option }
        override x.ToString() = genericToString x
    
    type Blueprint = 
        { ItemId : int64
          LocationId : LocationId
          TypeId : TypeId
          TypeName : string
          FlagId : int
          Quantity : int
          TimeEfficiency : int
          MaterialEfficiency : int
          Runs : int }
        override x.ToString() = genericToString x
    
    type Race = 
        | Minmatar = 0
        | Amarr = 1
        | Gallente = 2
        | Caldari = 3
    
    type Gender = 
        | Male = 0
        | Female = 1
    
    type Attributes = 
        { Intelligence : int
          Memory : int
          Charisma : int
          Perception : int
          Willpower : int }
        override x.ToString() = genericToString x
    
    type JumpClone = 
        { JumpCloneId : int
          TypeId : TypeId
          LocationId : LocationId
          CloneName : string }
        override x.ToString() = genericToString x
    
    type JumpCloneImplant = 
        { JumpCloneId : int
          TypeId : TypeId
          TypeName : string }
        override x.ToString() = genericToString x
    
    type Implant = 
        { TypeId : TypeId
          TypeName : string }
        override x.ToString() = genericToString x
    
    type Skill = 
        { TypeId : TypeId
          SkillPoints : int
          Level : int
          Published : int }
        override x.ToString() = genericToString x

    type Certificate =
        { CertificateId : int }
        override x.ToString() = genericToString x

    type CorporationRole = 
        { RoleId : int64
          RoleName : string }
    
    type CorporationTitle = 
        { TitleId : int
          TitleName : string }

    type CharacterSheet = 
        { CharacterId : int64
          Name : string
          HomeStationId : int64
          DateOfBirth : DateTime
          Race : Race
          BloodLineId : int64
          BloodLine : string
          AncestryId : int64
          Ancestry : string
          Gender : Gender
          CorporationName : string
          CorporationId : int64
          AllianceName : string
          AllianceId : int64
          FactionName : string
          FactionId : int64
          FreeSkillPoints : int64
          FreeRespecs : int
          CloneJumpDate : DateTime
          LastRespecDate : DateTime
          LastTimedRespec : DateTime
          JumpActiation : DateTime
          JumpFatigue : DateTime
          JumpLastUpdate : DateTime
          Balance : decimal
          Attributes : Attributes
          JumpClones : JumpClone list
          JumpCloneImplants : JumpCloneImplant list
          Implants : Implant list
          Skills : Skill list
          Certificates : Certificate list
          CorporationRoles : CorporationRole list
          CorporationRolesAtHq : CorporationRole list
          CorporationRolesAtBase : CorporationRole list
          CorporationRolesAtOther : CorporationRole list
          CorporationTitles : CorporationTitle list }
        override x.ToString() = genericToString x

module internal Calls = 
    open System
    open FSharp.Data
    open FSharpx.Collections
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
            result?accounts?>"rows"
            |> List.head
        
        let xa = row?attr
        { AccountId = mi xa?accountID
          AccountKey = mi xa?accountKey
          Balance = md xa?balance })
        |> handleResult xmlResult
    
    let AssetList xmlResult = 
        (fun result -> 
            let assetsRows = result?assets?>"rows"
            let rec readAssets assets = 
                assets
                |> List.map (fun asset -> 
                       let a = asset?attr
                       let content = (asset?contents?>"rows")
                       { ItemId = mi64 a?itemID
                         LocationId = match a?locationID with
                                      | Int64Value v -> Some(v)
                                      | _            -> None
                         TypeId = mi64 a?typeID
                         Quantity = mi a?quantity
                         Flag = mi a?flag
                         Singleton = mb a?singleton
                         RawQuantity = match a?rawQuantity with
                                       | IntValue v -> Some(v)
                                       | _          -> None
                         Content = readAssets (asset?contents?>"rows")
                         Item = None })

            readAssets assetsRows)
        |> handleResult xmlResult
    
    let Blueprints xmlResult = 
        (fun result -> 
            [ for row in result?blueprints?>"rows" do
                    let a = row?attr
                    yield { ItemId = mi64 a?itemID
                            LocationId = mi64 a?locationID
                            TypeId = mi64 a?typeID
                            TypeName = ms a?typeName
                            FlagId = mi a?flagID
                            Quantity = mi a?quantity
                            TimeEfficiency = mi a?timeEfficiency
                            MaterialEfficiency = mi a?materialEfficiency
                            Runs = mi a?runs } ])
        |> handleResult xmlResult
    
    let CharacterSheet xmlResult = 
        (fun result -> 
            { CharacterId = mi64 result?characterID
              Name = ms result?name
              HomeStationId = mi64 result?homeStationID
              DateOfBirth = mdt result?DoB
              Race = Enum.Parse(typeof<Race>, ms result?race) :?> Race
              BloodLineId = mi64 result?bloodLineID
              BloodLine = ms result?bloodLine
              AncestryId = mi64 result?ancestryID
              Ancestry = ms result?ancestry
              Gender = Enum.Parse(typeof<Gender>, ms result?gender) :?> Gender
              CorporationName = ms result?corporationName
              CorporationId = mi64 result?corporationID
              AllianceName = ms result?allianceName
              AllianceId = mi64 result?allianceID
              FactionName = ms result?factionName
              FactionId = mi64 result?factionID
              FreeSkillPoints = mi64 result?freeSkillPoints
              FreeRespecs = mi result?freeRespecs
              CloneJumpDate = mdt result?cloneJumpDate
              LastRespecDate = mdt result?lastRespecDate
              LastTimedRespec = mdt result?lastTimedRespec
              JumpActiation = mdt result?jumpActivation
              JumpFatigue = mdt result?jumpFatigue
              JumpLastUpdate = mdt result?jumpLastUpdate
              Balance = md result?balance
              Attributes = 
                  { Intelligence = mi result?attributes?intelligence
                    Memory = mi result?attributes?memory
                    Charisma = mi result?attributes?charisma
                    Perception = mi result?attributes?perception
                    Willpower = mi result?attributes?willpower }
              JumpClones = 
                [ for jc in result?>"jumpClones" do
                    yield { JumpCloneId = mi jc?attr?jumpCloneID
                            TypeId = mi64 jc?attr?typeID
                            LocationId = mi64 jc?attr?locationID
                            CloneName = ms jc?attr?typeName } ]
              JumpCloneImplants = List.empty<JumpCloneImplant>
              Implants = List.empty<Implant>
              Skills = 
                [ for skill in result?skills?>"rows" do
                    yield { TypeId = mi64 skill?attr?typeID
                            SkillPoints = mi skill?attr?skillpoints
                            Level = mi skill?attr?level
                            Published = mi skill?attr?published } ]
              Certificates = 
                [ for certificate in result?certificates?>"rows" do
                    yield { CertificateId = mi certificate?attr?certificateID } ]
              CorporationRoles = 
                [ for role in result?corporationRoles?>"rows" do
                    yield { RoleId = mi64 role?attr?roleID
                            RoleName = ms role?attr?roleName } ]
              CorporationRolesAtHq = 
                [ for role in result?corporationRolesAtHq?>"rows" do
                    yield { RoleId = mi64 role?attr?roleID
                            RoleName = ms role?attr?roleName } ]
              CorporationRolesAtBase = 
                [ for role in result?corporationRolesAtBase?>"rows" do
                    yield { RoleId = mi64 role?attr?roleID
                            RoleName = ms role?attr?roleName } ]
              CorporationRolesAtOther = 
                [ for role in result?corporationRolesAtOther?>"rows" do
                    yield { RoleId = mi64 role?attr?roleID
                            RoleName = ms role?attr?roleName } ]
              CorporationTitles = 
                [ for title in result?corporationTitles?>"rows" do
                    yield { TitleId = mi title?attr?titleID
                            TitleName = ms title?attr?titleName } ] })
        |> handleResult xmlResult
