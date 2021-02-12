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
            const int rateMs = 150;
            const string windowName = "One Finger Death Punch 2";

            try
            {
                var window = new WindowManager(windowName);
                var rect = window.Rectangle;
                Console.WriteLine($"{windowName} window: {rect}");

                var tracking = GetPoints(rect);
                Console.WriteLine($"Tracking: {tracking}");

                var screen = new Screenshooter(rect);
                using var recognizer = new Recognizer(tracking);

                Console.Write("Press Enter to start");
                Console.ReadLine();
                bool wasBrawler = false;
                while (true)
                {
                    var timeout = wasBrawler ? rateMs : rateMs + 50;
                    Thread.Sleep(timeout);

                    using Bitmap bmp = screen.MakeScreenshot();
                    (bool isLeft, bool isRight) = recognizer.Recognize(bmp, out wasBrawler);
                    if (isLeft)
                        window.TrySendClick(true);
                    if (isRight)
                        window.TrySendClick(false);
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

        private Tracking GetPoints(Rect rect)
        {
            // depends on screen and may be different
            // high graphics, windowed, 1280x720
            const decimal left1XCoeff = 0.46m;
            const decimal right1XCoeff = 0.53m;
            const decimal yCoeff = 0.71m;

            const decimal b_XCoeff = 0.5m;
            const decimal b_topYCoeff = 0.06m;
            const decimal b_botYCoeff = 0.364m;

            const decimal lhealthXCoeff = 0.436m;
            const decimal lhealthYCoeff = 0.055m;

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            var tracking = new Tracking(
                GetPoint(left1XCoeff, yCoeff),
                (0,0),
                GetPoint(right1XCoeff, yCoeff),
                (0,0),
                GetPoint(b_XCoeff, b_topYCoeff),
                GetPoint(b_XCoeff, b_botYCoeff),
                GetPoint(lhealthXCoeff, lhealthYCoeff)
            );

            return tracking;

            (int, int) GetPoint(decimal xCoeff, decimal yCoeff) => (
                    (int)(width * xCoeff),
                    (int)(height * yCoeff)
                );
        }
    }
}
