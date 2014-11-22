namespace Nebula.XML

open Nebula
open System
open System.Net
open System.Xml.Linq
open FSharp.Data
open FSharpx.Collections
open FSharpx.Http


exception EveApiException of int * string
exception ApiKeyRequiredException

type ApiServer =
    | Tranquility = 0
    | Singularity = 1

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

/// <summary>
/// This class is main class in the library allowing to
/// connect to EVE Online API (XML) and operate on it's methods.
/// </summary>
type Api(cache:Nebula.ICache, apiKey:APIKey option, apiServer:ApiServer) =

    let emptyParams = []

    let getNonAuthenticatedWebClient queryParams = 
        let webClient = new WebClient()
        webClient.QueryString <- queryParams |> NameValueCollection.ofSeq
        webClient

    let getAuthenticatedWebClient queryParams = 
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
    let queryApiServer url (webClient: WebClient) = 
        let getUri() =
            let baseUrl = match apiServer with
                          | ApiServer.Singularity -> "https://api.testeveonline.com/"
                          | ApiServer.Tranquility -> "https://api.eveonline.com/"
                          | _ -> ""
            new Uri(baseUrl + url)

        let getData() = 
            try
                async {
                    let! data = webClient.AsyncDownloadString(getUri())
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

    let authenticatedCall url parameters =
        getAuthenticatedWebClient parameters
        |> queryApiServer url 
      
    let nonAuthenticatedCall url parameters = 
        getNonAuthenticatedWebClient parameters
        |> queryApiServer url 

    /// <summary>
    /// API server 
    /// </summary>
    member x.ApiServer = apiServer

    /// <summary>
    /// Instance of ICache responsbile for caching API requests.
    /// </summary>
    member x.Cache = cache

    /// <summary>
    /// Instance of APIKey used to query API server
    /// </summary>
    member x.APIKey = apiKey

    /// <summary>
    /// Returns status of account. Requires API key.
    /// </summary>
    member x.AccountStatus() =
        authenticatedCall "/account/AccountStatus.xml.aspx" emptyParams
        |> API.Account.Calls.AccountStatus 

    /// <summary>
    /// Returns API key info. Requires API key.
    /// </summary>
    member x.AccountAPIKeyInfo() =
        authenticatedCall "/account/APIKeyInfo.xml.aspx" emptyParams
        |> API.Account.Calls.APIKeyInfo 

    /// <summary>
    /// Returns characters on account. Requires API key.
    /// </summary>
    member x.AccountCharacters() =
        let characters = authenticatedCall "/account/Characters.xml.aspx" emptyParams
                         |> API.Account.Calls.Characters

        for character in characters do
            character.Api <- x

        characters

    /// <summary>
    /// Returns map for factional warfare.
    /// </summary>
    member x.MapFactionalWarfareSystems() =
        nonAuthenticatedCall "/map/FacWarSystems.xml.aspx" emptyParams
        |> API.Map.Calls.FacWarSystems

    /// <summary>
    /// Returns jumps.
    /// </summary>
    member x.MapJumps() =
        nonAuthenticatedCall "/map/Jumps.xml.aspx" emptyParams
        |> API.Map.Calls.Jumps

    /// <summary>
    /// Returns kills.
    /// </summary>
    member x.MapKills() =
        nonAuthenticatedCall "/map/Kills.xml.aspx" emptyParams
        |> API.Map.Calls.Kills

    /// <summary>
    /// Returns sovereignty data.
    /// </summary>
    member x.MapSovereignty() =
        nonAuthenticatedCall "/map/Sovereignty.xml.aspx" emptyParams
        |> API.Map.Calls.Sovereignty

    /// <summary>
    /// Returns account balance for character. Requires API key.
    /// </summary>
    /// <param name="characterId">character id</param>
    member x.CharAccountBalance (characterId:int) =
        authenticatedCall "/char/AccountBalance.xml.aspx" [ "characterID", string(characterId) ]
        |> API.Character.Calls.AccountBalance

    /// <summary>
    /// Creates API object for querying EVE Online XML backend. Using Tranquility server by default.
    /// Some methods will throw <see cref="ApiKeyRequiredException">ApiKeyRequiredException</see> if they require API key to be executed.
    /// </summary>
    /// <param name="cache">ICache instance</param>
    new(cache:Nebula.ICache) = Api(cache, None, ApiServer.Tranquility)

    /// <summary>
    /// Creates API object for querying EVE Online XML backend. Using Tranquility server by default.
    /// </summary>
    /// <param name="cache">ICache instance</param>
    /// <param name="apiKey">API key</param>
    new(cache:Nebula.ICache, apiKey:APIKey) = Api(cache, Some(apiKey), ApiServer.Tranquility)

    /// <summary>
    /// Creates API object for querying EVE Online XML backend.
    /// </summary>
    /// <param name="cache">ICache instance</param>
    /// <param name="apiKey">API key</param>
    /// <param name="apiServer">API server</param>
    new(cache:Nebula.ICache, apiKey:APIKey, apiServer:ApiServer) = Api(cache, Some(apiKey), apiServer)

[<System.Runtime.CompilerServices.Extension>]
module CharacterExtensions =
    // C# way of adding extension methods...
    [<System.Runtime.CompilerServices.Extension>]
    let AccountBalance(c : API.Account.Records.Character) = 
        let api = c.Api :?> Api
        api.CharAccountBalance c.CharacterId       

    // F# way...
    type Nebula.XML.API.Account.Records.Character with
        member public x.AccountBalance() =
            let api = x.Api :?> Api
            api.CharAccountBalance x.CharacterId