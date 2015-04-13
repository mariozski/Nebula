module Nebula.ApiTypes 

open System.Dynamic
open System.Collections.Generic
open System
open System.Runtime.Serialization

type XmlEveError = 
    { Code:int; Message:string }

type XmlEveResponse = 
    { Result:ExpandoObject option; 
      Version: string; 
      CurrentTime:DateTime; 
      CachedUntil:DateTime;
      Error: XmlEveError option}
        
