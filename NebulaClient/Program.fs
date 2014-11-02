// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Nebula

[<EntryPoint>]
let main argv = 
    let api = new EveApi("http://api.eveonline.com", 
                        new APIKey(3806756, "Pc2FF5YnjFuRfHfEhda956k14x698J0FWcoP74Xtwom3EjaASKqGYumY3HrXn0p4"), 
                        new NoCache())

    try
        let status = api.AccountStatus()
        printf "%O" status.LogonMinutes
    with
    | :? EveApiException as ex ->
        printf "Error occured: %O %O" ex.Data0 ex.Data1

    System.Console.ReadKey() |> ignore
    0 // return an integer exit code
