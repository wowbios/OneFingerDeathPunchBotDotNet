module Screenshooter

    open System.Drawing
    open System.Drawing.Imaging
    open OFDPBot.FSharp

    let public MakeScreenshot(rect : InteropNative.Rect) =
        let width = rect.Right - rect.Left
        let height = rect.Bottom - rect.Top
        let bmp = Bitmap(width, height, PixelFormat.Format32bppArgb)
        use graphics = Graphics.FromImage bmp
        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, Size(width, height), CopyPixelOperation.SourceCopy)
        bmp