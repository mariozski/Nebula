namespace Nebula

open System
open System.Xml.Linq

type MemoryCache private() =
    // using built-in runtime caching of .NET
    let cache = new System.Runtime.Caching.MemoryCache("memorycache", null)

    interface ICache with
        member x.Get key =
            let value = cache.Get(key)
            match value with
            | null -> None
            | _ -> printfn "Reading from cache %s" key
                   let casted = value :?> (XElement * DateTimeOffset)              
                   Some(fst casted)

        member x.Set key value expiration =
            let cacheExpiration = DateTimeOffset.Now + expiration
            printfn "Setting cache %s" key
            cache.Set(key, (value, cacheExpiration), cacheExpiration)

    /// <summary>
    /// Returns number of seconds after which cache item will expire
    /// </summary>
    /// <param name="key">key for cache</param>
    member x.GetExpiration key =
        let value = cache.Get(key)
        match value with
        | null -> 0
        | _ -> let casted = value :?> (XElement * DateTimeOffset)              
               (snd casted).Second

    /// <summary>
    /// Memory cache object instance
    /// </summary>
    static member val Instance = MemoryCache()

