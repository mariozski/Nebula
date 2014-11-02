namespace Nebula.Cache

open System
open System.Xml.Linq

/// <summary>
/// Interface used to implement caching solution.
/// </summary>
type ICache =
    /// <summary>
    /// Sets value in cache for given key, while setting absolute expiration time
    /// </summary>
    abstract member Set : key:string -> value:XElement -> expiration:TimeSpan ->  unit

    /// <summary>
    /// Gets value from cache for given key. Should return None if elements is not found
    /// </summary>
    abstract member Get : key:string -> XElement option

