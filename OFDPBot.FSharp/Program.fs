// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open OFDPBot.FSharp

[<Literal>]
let rate = 150

[<Literal>]
let windowName = "Steam" //"One Finger Death Punch 2"

let start() =
     WindowManager.getHWnd windowName
     |> Result.mapError raise // можно завернуть в computation expression и скрыть из логики
     |> Result.bind WindowManager.getRect
     |> Result.mapError raise // можно завернуть в computation expression и скрыть из логики
     |> Result.iter (fun rect -> // можно завернуть в computation expression и скрыть из логики
        let track = Tracking.get rect
        printfn "%s window : %A\nTracking %A" windowName rect track
        use screenshot = Screenshooter.makeScreenshot rect
        screenshot.Save("test.bmp"))

[<EntryPoint>]
let main argv =
    start()
    0 // return an integer exit code