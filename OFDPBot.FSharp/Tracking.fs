module Tracking

open OFDPBot.FSharp

type [<Struct>] Point = {
    X: int
    Y: int
}

type [<Struct>] BrawlerTracking = {
    Top: Point
    Bottom: Point
}

type [<Struct>] Tracking = {
    Left: Point
    Right: Point
    HealthBar: Point
    BrawlerTracking: BrawlerTracking
}

let [<Literal>] private left1XCoeff = 0.46

let [<Literal>] private right1XCoeff = 0.53

let [<Literal>] private yCoeff = 0.71

let [<Literal>] private b_XCoeff = 0.5

let [<Literal>] private b_topYCoeff = 0.06

let [<Literal>] private b_botYCoeff = 0.364

let [<Literal>] private lhealthXCoeff = 0.436

let [<Literal>] private lhealthYCoeff = 0.055

let get (rect: InteropNative.Rect) =
    let w = float(rect.Right - rect.Left)
    let h = float(rect.Bottom - rect.Top)
    let point xc yc = { X = int(xc * w); Y = int(h * yc) }
    {
        Left = point left1XCoeff yCoeff
        Right = point right1XCoeff yCoeff
        HealthBar = point lhealthXCoeff lhealthYCoeff
        BrawlerTracking = {
            Top = point b_XCoeff b_topYCoeff
            Bottom = point b_XCoeff b_botYCoeff
        }
    }