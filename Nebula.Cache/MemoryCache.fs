﻿namespace Nebula.Cache

open Nebula
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
            | _ -> let casted = value :?> (XElement * DateTimeOffset)              
#if DEBUG
                   printfn "\nReading from cache %s\n" key
#endif
                   Some(fst casted)

        member x.Set key value expiration =
            let cacheExpiration = DateTimeOffset.Now + expiration
#if DEBUG
            printfn "\nSetting cache %s\n" key
#endif
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
