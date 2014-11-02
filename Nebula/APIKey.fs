namespace Nebula

open FSharpx.Collections

type APIKey = 
    { KeyId:int; VerificationCode:string }
    member x.ToNameValueCollection = 
        [ "keyID", string(x.KeyId);
          "vCode", x.VerificationCode ]
        |> NameValueCollection.ofSeq