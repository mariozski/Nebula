﻿namespace Nebula.Cache

open System
open System.Xml.Linq

type NoCache() =

    interface ICache with
        member x.Get key =
            None

        member x.Set key value expiration =
            ()

