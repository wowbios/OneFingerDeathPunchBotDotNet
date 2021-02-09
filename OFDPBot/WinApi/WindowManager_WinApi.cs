using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable 649
namespace OFDPBot.WinApi
{
    internal partial class WindowManager
    {

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        private static bool Click(bool isLeft)
        {
            const uint LEFT_DOWN = 0x0002;
            const uint LEFT_UP = 0x0004;
            const uint RIGHT_DOWN = 0x0008;
            const uint RIGHT_UP = 0x0010;

            INPUT inputMouseDown = CreateInput(isLeft ? LEFT_DOWN : RIGHT_DOWN);
            INPUT inputMouseUp = CreateInput(isLeft ? LEFT_UP : RIGHT_UP);

            var inputs = new INPUT[] { inputMouseDown };
            bool isDownSuccessful = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) != 0;
            if (!isDownSuccessful)
                return false;

            Thread.Sleep(1); // must wait a little

            inputs = new INPUT[] { inputMouseUp };
            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) != 0;
        }
    }
}