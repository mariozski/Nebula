namespace Nebula

open System
open System.Net
open FSharp.Data
open FSharpx.Collections
open FSharpx.Http

type ApiResponse = XmlProvider<"""<eveapi version="2">
                                        <currentTime>2010-10-05 20:28:28</currentTime>
                                        <result></result>
                                        <error code="106">Must provide userID or keyID parameter for authentication.</error>
                                        <cachedUntil>2010-10-05 20:28:28</cachedUntil>
                                   </eveapi>""">

/// <summary>
/// This class is main class in the library allowing to
/// connect to EVE Online API (XML) and operate on it's methods.
/// </summary>
type EveApi(baseUrl, cache:ICache) = 
    
    /// <summary>
    /// Base url to EVE Online API backend.
    /// </summary>
    member x.BaseUrl = baseUrl

    /// <summary>
    /// Instance of ICache responsbile for caching API requests.
    /// </summary>
    member x.Cache = cache

    /// <summary>
    /// Queries EVE Online API for given request type and parameters
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="id"></param>
    /// <param name="key"></param>
    /// <param name="additionalParameters"></param>
    member private x.QueryResult (apiKey:APIKey) path additionalParameters = 
        let MakeApiRequest = 
            async {
                let webClient = new WebClient()
                let nvc = [ "keyID", string(apiKey.KeyID); "vCode", apiKey.VerificationCode ] 
                          |> List.append additionalParameters
                          |> NameValueCollection.ofSeq 

                webClient.QueryString <- nvc
                let! data = webClient.AsyncDownloadString(new Uri(x.BaseUrl + path))
                return data
            }

        let addString = additionalParameters |> List.fold (fun acc x -> acc + "||" + 
                                                                        match x with
                                                                        | (a, b) -> a + "|" + b) ""

        let cacheKey = path + string(apiKey.KeyID) + apiKey.VerificationCode + addString
                       

        // get xml data for query
        let xml = match x.Cache.Get(cacheKey) with
                    | Some(result) -> result
                    | None -> 
                        let response = Async.RunSynchronously MakeApiRequest
                        let parsedResponse = ApiResponse.Parse(response)
                        let result = parsedResponse.Result.XElement
                        x.Cache.Set cacheKey result (parsedResponse.CachedUntil - parsedResponse.CurrentTime)
                        result
                    
        // parse xml and create expando object with proper structure
        xml   

    



