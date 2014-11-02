namespace Nebula.XML

open Nebula
open System
open System.Net
open System.Xml.Linq
open FSharp.Data
open FSharpx.Collections
open FSharpx.Http

type ApiResponse = XmlProvider<"""<root><eveapi version="2">
                                        <currentTime>2010-10-05 20:28:28</currentTime>
                                        <result></result>
                                        <cachedUntil>2010-10-05 20:28:28</cachedUntil>
                                   </eveapi>
                                   <eveapi version="2">
                                        <currentTime>2010-10-05 20:28:28</currentTime>
                                        <error code="106">Must provide userID or keyID parameter for authentication.</error>
                                        <cachedUntil>2010-10-05 20:28:28</cachedUntil>
                                   </eveapi></root>""", SampleIsList=true>

exception EveApiException of int * string

/// <summary>
/// This class is main class in the library allowing to
/// connect to EVE Online API (XML) and operate on it's methods.
/// </summary>
type Api(baseUrl, apiKey:APIKey, cache:ICache) = 
    
    /// <summary>
    /// Base url to EVE Online API backend.
    /// </summary>
    member x.BaseUrl = baseUrl

    /// <summary>
    /// Instance of ICache responsbile for caching API requests.
    /// </summary>
    member x.Cache = cache

    /// <summary>
    /// Instance of APIKey used to query Eve Online API
    /// </summary>
    member x.APIKey = apiKey

    /// <summary>
    /// Queries EVE Online API for given request type and parameters
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="id"></param>
    /// <param name="key"></param>
    /// <param name="additionalParameters"></param>
    member private x.QueryResult path additionalParameters = 
        let MakeApiRequest = 
            try
                async {
                    let webClient = new WebClient()
                    let nvc = [ "keyID", string(apiKey.KeyId); "vCode", apiKey.VerificationCode ] 
                              |> List.append additionalParameters
                              |> NameValueCollection.ofSeq 

                    webClient.QueryString <- nvc
                    let! data = webClient.AsyncDownloadString(new Uri(x.BaseUrl + path))
                    return data
                } |> Async.RunSynchronously
            with
            | :? WebException as ex ->
                (new System.IO.StreamReader(ex.Response.GetResponseStream())).ReadToEnd()

        let addString = additionalParameters |> List.fold (fun acc x -> acc + "||" + 
                                                                        match x with
                                                                        | (a, b) -> a + "|" + b) ""
        let cacheKey = path + string(apiKey.KeyId) + apiKey.VerificationCode + addString
        // get xml data for query
        let xml = match x.Cache.Get(cacheKey) with
                    | Some(result) -> result
                    | None -> 
                        let parsedResponse = ApiResponse.Parse MakeApiRequest
                        match parsedResponse.Result with
                        | Some(result) -> x.Cache.Set cacheKey result.XElement (parsedResponse.CachedUntil - parsedResponse.CurrentTime)
                                          result.XElement   
                        | None -> if parsedResponse.Error.IsSome then
                                    let error = parsedResponse.Error.Value
                                    raise (EveApiException (error.Code, error.Value))
                                    error.XElement
                                  else
                                    new XElement(XName.Get "")

        // return xml entry of result
        string(xml)

    member x.AccountStatus() =
        API.Account.Calls.AccountStatus (x.QueryResult "/account/AccountStatus.xml.aspx" [])
    
    member x.AccountAPIKeyInfo() =
        API.Account.Calls.APIKeyInfo (x.QueryResult "/account/APIKeyInfo.xml.aspx" [])

    member x.AccountCharacters() =
        API.Account.Calls.Characters (x.QueryResult "/account/Characters.xml.aspx" [])

    member x.MapFactionalWarfareSystems() =
        API.Map.Calls.FacWarSystems (x.QueryResult "/map/FacWarSystems.xml.aspx" [])

    member x.MapJumps() =
        API.Map.Calls.Jumps (x.QueryResult "/map/Jumps.xml.aspx" [])

    member x.MapKills() =
        API.Map.Calls.Kills (x.QueryResult "/map/Kills.xml.aspx" [])

    member x.MapSovereignty() =
        API.Map.Calls.Sovereignty (x.QueryResult "/map/Sovereignty.xml.aspx" [])
