using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace OFDPBot
{
    internal class WindowFinder
    {
        private readonly string _windowName;
        private readonly IntPtr _hwnd;

        public WindowFinder(string windowName)
        {
            if (string.IsNullOrEmpty(windowName))
            {
                throw new ArgumentException($"'{nameof(windowName)}' cannot be null or empty.", nameof(windowName));
            }

            _windowName = windowName;

            _hwnd = FindWindow(null, _windowName);
            if (_hwnd == IntPtr.Zero)
                throw new Exception($"Cant find window {_windowName}");

            var rect = default(Rect);
            if (!GetWindowRect(_hwnd, ref rect))
                throw new Exception("Cant get window rect");

            Rectangle = rect;
        }

        public Rect Rectangle { get; }
#region winapi

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

#endregion

        public bool TrySendClick(bool isLeft)
        {
            if (!IsMouseInTargetWindow()) 
                return false;

            Click(isLeft);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseInTargetWindow()
        {
            var point = GetCursorPosition();
            return point.X > Rectangle.Left
                && point.X < Rectangle.Right
                && point.Y > Rectangle.Top
                && point.Y < Rectangle.Bottom;
        }

        public static Point GetCursorPosition()
        {
            if (!GetCursorPos(out MousePoint currentMousePoint))
                throw new Exception("Cant get mouse point");

            return new Point(currentMousePoint.X, currentMousePoint.Y);
        }

        public static bool Click(bool isLeft)
        {
            const uint LEFT_DOWN = 0x0002;
            const uint LEFT_UP = 0x0004;
            const uint RIGHT_DOWN = 0x0008;
            const uint RIGHT_UP = 0x0010;
            var inputMouseDown = CreateInput(isLeft ? LEFT_DOWN : RIGHT_DOWN);
            var inputMouseUp = CreateInput(isLeft ? LEFT_UP : RIGHT_UP);

            var inputs = new INPUT[] { inputMouseDown };
            bool firstOk = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) != 0;
            if (!firstOk)
                return false;
            
            Thread.Sleep(1);
            inputs = new INPUT[] { inputMouseUp };
            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static INPUT CreateInput(uint flags)
        {
            var input = new INPUT
            {
                Type = 0
            };
            input.Data.Mouse.Flags = flags;
            return input;
        }
    }
}