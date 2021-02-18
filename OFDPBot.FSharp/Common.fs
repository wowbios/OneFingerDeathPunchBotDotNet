namespace OFDPBot.FSharp

module Result =
    let iter f result =
        match result with
        | Ok value -> f value
        | _ -> ()