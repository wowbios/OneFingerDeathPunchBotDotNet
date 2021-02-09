using System.Runtime.InteropServices;

namespace OFDPBot
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner

        public override string ToString()
            => $"L={Left} R={Right} T={Top} B={Bottom}";
    }
}