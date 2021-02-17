namespace OFDPBot.FSharp

open System
open System.Threading
open System.Runtime.InteropServices


module InteropNative =

    [<StructLayout(LayoutKind.Sequential)>]
    type public Rect =
        struct
            val Left : int
            val Top : int
            val Right : int
            val Bottom : int
            override this.ToString() =
                sprintf "L={%d} R={%d} T={%d} B={%d}" this.Left this.Right this.Top this.Bottom
        end

    [<StructLayout(LayoutKind.Sequential)>]
    type MousePoint =
        struct
            val X : int
            val Y : int
            new(x : int, y : int) = { X = x; Y = y }
        end

    type MOUSEINPUT =
        struct 
            val X : int
            val Y : int
            val MouseData : uint
            val mutable Flags : uint
            val Time : uint
            val ExtraInfo : unativeint
        end

    [<StructLayout(LayoutKind.Explicit)>]
    type MOUSEKEYBDHARDWAREINPUT =
        struct
            [<FieldOffset(0)>]
            val mutable Mouse : MOUSEINPUT
        end

    type INPUT =
        struct
            val Type : unativeint
            val mutable Data : MOUSEKEYBDHARDWAREINPUT
        end

    [<DllImport("user32.dll", SetLastError = true)>]
    extern IntPtr FindWindow(string lpClassName, string lpWindowName)

    [<DllImport("user32.dll", SetLastError = true)>]
    [<MarshalAs(UnmanagedType.Bool)>]
    extern bool GetWindowRect(IntPtr hWnd, Rect& rect)

    [<DllImport("user32.dll")>]
    [<MarshalAs(UnmanagedType.Bool)>]
    extern bool GetCursorPos(MousePoint& lpMousePoint)

    [<DllImport("user32.dll")>]
    extern uint SendInput(uint nInputs, [<MarshalAs(UnmanagedType.LPArray)>][<In>] INPUT[] pInputs, int cbSize);


    [<Literal>]
    let LEFT_DOWN = 0x0002u

    [<Literal>]
    let LEFT_UP = 0x0004u

    [<Literal>]
    let RIGHT_DOWN = 0x0008u

    [<Literal>]
    let RIGHT_UP = 0x0010u

    let public GetCursorPosition =
        let mutable position = MousePoint()
        GetCursorPos(&position) |> ignore
        position

    let private createInput flags =
        let input = INPUT()
        input.Data.Mouse.Flags <- flags
        input

    let private SendInputToWindow input =
        let inputs = [| input |]
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) <> 0u
    
    let public Click isLeft =
        let down = if isLeft then LEFT_DOWN else RIGHT_DOWN
        let up = if isLeft then LEFT_UP else LEFT_DOWN
        let mouseDown = createInput down
        let mouseUp = createInput up

        if SendInputToWindow mouseDown then
            Thread.Sleep(1)
            SendInputToWindow mouseUp
        else
            false

module WindowManager =

    let private isMouseInTargetWindow(rect : InteropNative.Rect) = 
        let mouse = InteropNative.GetCursorPosition
        mouse.X > rect.Left && mouse.X < rect.Right && mouse.Y > rect.Top && mouse.Y < rect.Bottom

    let private ClickInArea rect isLeft =
        if isMouseInTargetWindow rect then InteropNative.Click isLeft |> ignore

    let public GetHWnd name = InteropNative.FindWindow(null, name)
    
    let public RightClick rect = ClickInArea rect false

    let public LeftClick rect = ClickInArea rect true
    
    let public GetRect hwnd = 
        let mutable rect = InteropNative.Rect()
        InteropNative.GetWindowRect(hwnd, &rect) |> ignore
        rect