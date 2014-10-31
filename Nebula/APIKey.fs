namespace Nebula

open FSharpx.Collections

type APIKey(keyID:int, vCode:string) =
    member x.KeyID = keyID
    member x.VerificationCode = vCode
    member x.ToNameValueCollection = 
        [ "keyID", string(x.KeyID);
          "vCode", x.VerificationCode ]
        |> NameValueCollection.ofSeq