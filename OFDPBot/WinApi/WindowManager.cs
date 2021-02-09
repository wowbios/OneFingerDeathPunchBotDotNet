using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace OFDPBot.WinApi
{
    internal partial class WindowManager
    {
        private readonly string _windowName;
        private readonly IntPtr _hwnd;

        public WindowManager(string windowName)
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

        public bool TrySendClick(bool isLeft)
        {
            if (!IsMouseInTargetWindow()) 
                return false;

            Click(isLeft);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsMouseInTargetWindow()
        {
            var point = GetCursorPosition();
            return point.X > Rectangle.Left
                && point.X < Rectangle.Right
                && point.Y > Rectangle.Top
                && point.Y < Rectangle.Bottom;
        }

        private static Point GetCursorPosition()
        {
            if (!GetCursorPos(out MousePoint currentMousePoint))
                throw new Exception("Cant get mouse point");

            return new Point(currentMousePoint.X, currentMousePoint.Y);
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