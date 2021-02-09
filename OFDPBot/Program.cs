using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace OFDPBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Start();
        }

        private void Start()
        {
            const int rateMs = 100;
            const string windowName = "One Finger Death Punch";
            Process process = null;
            LowLevelKeyboardHook lowLevelHook = null;
            int hooks = 0;
            try
            {
                var window = new WindowFinder(windowName);
                var rect = window.Rectangle;
                Console.WriteLine($"{windowName} window: {rect}");

                process = Process.GetProcessesByName(windowName)[0];

                var (left, right) = GetPoints(rect);
                Console.WriteLine($"POINTS:\nLEFT {left}\nRIGHT {right}");

                var screen = new Screenshooter(rect);
                var recognizer = new Recognizer(left, right);

                Console.Write("Press Enter to start");
                Console.ReadLine();
                while (true)
                {
                    using Bitmap bmp = screen.Make();
                    (bool isLeft, bool isRight) = recognizer.Recognize(bmp);
                    if (isLeft && window.TrySendClick(true))
                    {
                        Console.WriteLine("LEFT");
                    }
                    else if (isRight && window.TrySendClick(false))
                    {
                        Console.WriteLine("RIGHT");
                    }
                    else
                    {
                        Console.WriteLine("NONE");
                    }

                    Thread.Sleep(rateMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
            finally
            {
                lowLevelHook?.UnHookKeyboard();
                process?.Dispose();
                Console.WriteLine("END" + hooks);
            }
        }

        private ((int lx, int ly), (int rx, int ry)) GetPoints(Rect rect)
        {
            const decimal leftXCoeff = 2.27m;
            const decimal rightXCoeff = 1.829m;
            const decimal yCoeff = 1.91m;

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            ((int lx, int ly), (int rx, int ry)) screenPoint = (
                (
                    (int)(width / leftXCoeff),
                    (int)(height / yCoeff)
                ), (
                    (int)(width / rightXCoeff), 
                    (int)(height/yCoeff)
                ));

            Console.WriteLine($"SCREEN POINT: {screenPoint}");

            return screenPoint;
        }
    }
}
