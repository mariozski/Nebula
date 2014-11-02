module Nebula.XML.API.Shared

let internal genericToString (x:obj) = 
    let t = x.GetType()
    (t.GetProperties() |> Array.fold (fun acc prop -> acc + prop.Name + "=" + string(prop.GetValue(x, null)) + ";") "").TrimEnd(';')


