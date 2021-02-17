// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open OFDPBot.FSharp

[<Literal>]
let rate = 150

[<Literal>]
let windowName = "Steam" //"One Finger Death Punch 2"

let start = 
    let hwnd = WindowManager.GetHWnd windowName
    let rect = WindowManager.GetRect hwnd
    let track = Tracking.Get rect
    printfn "%s window : %A\nTracking %A" windowName rect track
    use screenshot = Screenshooter.MakeScreenshot rect
    screenshot.Save("test.bmp")

[<EntryPoint>]
let main argv =
    start
    0 // return an integer exit code
