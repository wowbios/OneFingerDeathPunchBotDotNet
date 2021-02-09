using System;
using System.Drawing;
using System.Threading;
using OFDPBot.WinApi;

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

            try
            {
                var window = new WindowManager(windowName);
                var rect = window.Rectangle;
                Console.WriteLine($"{windowName} window: {rect}");

                var (left, right) = GetPoints(rect);
                Console.WriteLine($"POINTS:\nLEFT {left}\nRIGHT {right}");

                var screen = new Screenshooter(rect);
                var recognizer = new Recognizer(left, right);

                Console.Write("Press Enter to start");
                Console.ReadLine();
                while (true)
                {
                    Thread.Sleep(rateMs);

                    using Bitmap bmp = screen.MakeScreenshot();
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
            finally
            {
                Console.WriteLine("END");
            }
        }

        private ((int lx, int ly), (int rx, int ry)) GetPoints(Rect rect)
        {
            // depends on screen and may be different
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
