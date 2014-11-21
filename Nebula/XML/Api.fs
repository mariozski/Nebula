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
exception ApiKeyRequiredException

module ApiFunctions =

    let getNonAuthenticatedWebClient queryParams = 
        let webClient = new WebClient()
        webClient.QueryString <- queryParams |> NameValueCollection.ofSeq
        webClient

    let getAuthenticatedWebClient (apiKey:APIKey option) queryParams = 
        match apiKey with
        | Some(key) ->
            let webClient = new WebClient()
            let nvc = [ "keyID", string(key.KeyId); "vCode", key.VerificationCode ] 
                        |> List.append queryParams
                        |> NameValueCollection.ofSeq 

            webClient.QueryString <- nvc
            webClient
        | None -> raise ApiKeyRequiredException

        /// Queries EVE Online API for given request type and parameters
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="id"></param>
    /// <param name="key"></param>
    /// <param name="additionalParameters"></param>
    let queryApiServer (cache:ICache) url (webClient:WebClient) = 
        let getData() = 
            try
                async {
                    let! data = webClient.AsyncDownloadString(url)
                    return data
                } |> Async.RunSynchronously
            with
            | :? WebException as ex ->
                (new System.IO.StreamReader(ex.Response.GetResponseStream())).ReadToEnd()

        let cacheKey = webClient.QueryString 
                        |> NameValueCollection.toList 
                        |> List.append ["url", url.ToString()]
                        |> List.fold (fun acc x -> acc + "||" + 
                                                   match x with
                                                   | (a, b) -> a + "|" + b) ""

        // get xml data for query
        let xml = match cache.Get(cacheKey) with
                    | Some(result) -> result
                    | None -> 
                        let parsedResponse = getData() |> ApiResponse.Parse
                        match parsedResponse.Result with
                        | Some(result) -> cache.Set cacheKey result.XElement (parsedResponse.CachedUntil - parsedResponse.CurrentTime)
                                          result.XElement   
                        | None -> if parsedResponse.Error.IsSome then
                                    let error = parsedResponse.Error.Value
                                    raise (EveApiException (error.Code, error.Value))
                                    error.XElement
                                  else
                                    new XElement(XName.Get "")

        // return xml entry of result
        string(xml)

open ApiFunctions

/// <summary>
/// This class is main class in the library allowing to
/// connect to EVE Online API (XML) and operate on it's methods.
/// </summary>
type Api(baseUrl, cache:ICache, ?apiKey:APIKey) =

    let getFullUrl url = 
        new Uri(baseUrl + url)
    
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

    member x.AccountStatus() =
        ApiFunctions.getAuthenticatedWebClient x.APIKey [] 
        |> ApiFunctions.queryApiServer x.Cache (getFullUrl "/account/AccountStatus.xml.aspx")
        |> API.Account.Calls.AccountStatus 

    member x.AccountAPIKeyInfo() =
        getAuthenticatedWebClient x.APIKey [] 
        |> queryApiServer x.Cache (getFullUrl "/account/APIKeyInfo.xml.aspx")
        |> API.Account.Calls.APIKeyInfo 

    member x.AccountCharacters() =
        getAuthenticatedWebClient x.APIKey [] 
        |> queryApiServer x.Cache (getFullUrl "/account/Characters.xml.aspx")
        |> API.Account.Calls.Characters

    member x.MapFactionalWarfareSystems() =
        getNonAuthenticatedWebClient [] 
        |> queryApiServer x.Cache (getFullUrl "/map/FacWarSystems.xml.aspx")
        |> API.Map.Calls.FacWarSystems

    member x.MapJumps() =
        getNonAuthenticatedWebClient [] 
        |> queryApiServer x.Cache (getFullUrl "/map/Jumps.xml.aspx")
        |> API.Map.Calls.Jumps

    member x.MapKills() =
        getNonAuthenticatedWebClient [] 
        |> queryApiServer x.Cache (getFullUrl "/map/Kills.xml.aspx")
        |> API.Map.Calls.Kills

    member x.MapSovereignty() =
        getNonAuthenticatedWebClient [] 
        |> queryApiServer x.Cache (getFullUrl "/map/Sovereignty.xml.aspx")
        |> API.Map.Calls.Sovereignty

    member x.CharAccountBalance (characterId:int) =
        getAuthenticatedWebClient x.APIKey [ "characterID", string(characterId) ] 
        |> queryApiServer x.Cache (getFullUrl "/char/AccountBalance.xml.aspx")
        |> API.Character.Calls.AccountBalance