namespace Nebula

open System.Net

/// <summary>
/// This class is main class in the library allowing to
/// connect to EVE Online API (XML) and operate on it's methods
/// </summary>
type EveApiConnection(url) = 
    
    /// <summary>
    /// Url to EVE Online API backend
    /// </summary>
    member this.Url = url

    member this.Query = 
        let webClient = new WebClient()
        
        0



