namespace Nebula.Cache

open Nebula
open System
open System.Xml.Linq
open StackExchange.Redis

type RedisCache(redisConnectionString:string) =
    let redis = Async.AwaitTask<ConnectionMultiplexer>(ConnectionMultiplexer.ConnectAsync(redisConnectionString))
                |> Async.RunSynchronously

    interface ICache with
        member x.Get (key:string) =
            let db = redis.GetDatabase()
            let value = db.StringGet(RedisKey.op_Implicit key)
            match value.IsNullOrEmpty with
            | true -> None
            | false -> let value = string(value)
#if DEBUG
                       printfn "\nReading from cache %s" key            
#endif
                       Some(value)

        member x.Set key value expiration =
            let db = redis.GetDatabase()
            let cacheExpiration = DateTimeOffset.Now + expiration
#if DEBUG
            printfn "\nSetting cache %s" key
#endif
            db.StringSet(RedisKey.op_Implicit(key), RedisValue.op_Implicit(value), new Nullable<TimeSpan>(expiration)) |> ignore

