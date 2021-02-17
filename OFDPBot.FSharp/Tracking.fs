module Tracking

open OFDPBot.FSharp

type Point = 
    struct
        val X : int
        val Y : int
        new (x:int, y:int) = { X = x; Y = y }
    end

type BrawlerTracking =
    struct
        val Top : Point
        val Bottom : Point
        new (top : Point, bottom: Point) =
            {
                Top = top;
                Bottom = bottom;
            }
    end

type Tracking =
    struct
        val Left : Point
        val Right : Point
        val HealthBar : Point
        val BrawlerTracking : BrawlerTracking
        new (left:Point, right:Point, healthBar: Point, brawlerTracking: BrawlerTracking) =
            {
                Left = left;
                Right = right;
                HealthBar = healthBar;
                BrawlerTracking = brawlerTracking;
            }
    end

[<Literal>]
let private left1XCoeff = 0.46
[<Literal>]
let private right1XCoeff = 0.53
[<Literal>]
let private yCoeff = 0.71
[<Literal>]
let private b_XCoeff = 0.5
[<Literal>]
let private b_topYCoeff = 0.06
[<Literal>]
let private b_botYCoeff = 0.364
[<Literal>]
let private lhealthXCoeff = 0.436
[<Literal>]
let private lhealthYCoeff = 0.055

let public Get(rect : InteropNative.Rect) =

    let w = float(rect.Right - rect.Left)
    let h = float(rect.Bottom - rect.Top)
    let getPoint xc yc = Point(int(xc * w), int(h * yc))

    Tracking(
        getPoint left1XCoeff yCoeff,
        getPoint right1XCoeff yCoeff,
        getPoint lhealthXCoeff lhealthYCoeff,
        BrawlerTracking(
            getPoint b_XCoeff b_topYCoeff,
            getPoint b_XCoeff b_botYCoeff
        ))