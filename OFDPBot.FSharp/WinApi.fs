namespace OFDPBot.FSharp

open System
open System.ComponentModel
open System.Threading
open System.Runtime.InteropServices

module InteropNative =

    [<StructLayout(LayoutKind.Sequential)>] // можно попробовать рекорды юзать тоже, но не знаю как они будут работать
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

    let [<Literal>] LEFT_DOWN = 0x0002u
    let [<Literal>] LEFT_UP = 0x0004u
    let [<Literal>] RIGHT_DOWN = 0x0008u
    let [<Literal>] RIGHT_UP = 0x0010u

    let getCursorPosition() =
        let mutable position = MousePoint()
        GetCursorPos(&position) |> ignore
        position

    let private createInput flags =
        let input = INPUT()
        input.Data.Mouse.Flags <- flags
        input

    let private sendInputToWindow input =
        let inputs = [| input |]
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) <> 0u
    
    let mouseClick isLeft =
        let down = if isLeft then LEFT_DOWN else RIGHT_DOWN
        let up = if isLeft then LEFT_UP else LEFT_DOWN
        let mouseDown = createInput down
        let mouseUp = createInput up

        if sendInputToWindow mouseDown then
            Thread.Sleep(1)
            sendInputToWindow mouseUp
        else
            false

module WindowManager =
    let private (|Eq|_|) v2 v1 = if v1 = v2 then Some() else None

    let private isMouseInTargetWindow(rect : InteropNative.Rect) = 
        let mouse = InteropNative.getCursorPosition()
        mouse.X > rect.Left && mouse.X < rect.Right && mouse.Y > rect.Top && mouse.Y < rect.Bottom

    let private clickInArea rect isLeft =
        if isMouseInTargetWindow rect then InteropNative.mouseClick isLeft |> ignore
    
    let rightClick rect = clickInArea rect false

    let leftClick rect = clickInArea rect true

    let getHWnd name =
        match InteropNative.FindWindow(null, name) with
        | Eq IntPtr.Zero -> Marshal.GetLastWin32Error() |> Win32Exception |> Error
        | hwnd -> Ok hwnd

    let getRect hwnd =
        let mutable rect = InteropNative.Rect()
        match InteropNative.GetWindowRect(hwnd, &rect) with
        | false -> Marshal.GetLastWin32Error() |> Win32Exception |> Error
        | _ -> Ok rect