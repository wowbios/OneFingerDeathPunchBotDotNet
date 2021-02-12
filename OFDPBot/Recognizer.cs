using System;
using System.Drawing;

namespace OFDPBot
{
    internal class Recognizer
    {
        private readonly Tracking _tracking;

        public Recognizer(Tracking tracking)
        {
            _tracking = tracking;
        }

        public (bool, bool) Recognize(Bitmap bmp, out bool isBrawler)
        {
            // bool isBrawler = !Is(_tracking.LeftHealth, IsRed, bmp);
            // Console.WriteLine("IS BRAWLER = " + isBrawler);
            // if (isBrawler)
            //     CheckBrawler(bmp);
            // return (false, false);
            isBrawler = false;
            if (Is(_tracking.Left1, IsBlue, bmp))
            {
                Console.WriteLine("LEFT");
                return (true, false);
            }

            if (Is(_tracking.Right1, IsRed, bmp))
            {
                Console.WriteLine("RIGHT");
                return (false, true);
            }
            
            // bool leftGrey = Is(_tracking.Left1, IsGrey, bmp);
            // bool rightGrey = Is(_tracking.Right1, IsGrey, bmp);
            // if (leftGrey && rightGrey)

            isBrawler = !Is(_tracking.LeftHealth, IsRed, bmp);
            if (!isBrawler)
            {
                // Console.WriteLine($"NOTHING-");
                return (false, false);
            }

            return CheckBrawler(bmp);
        }

        private static int brawlerCounter = 0;

        private (bool, bool) CheckBrawler(Bitmap bmp)
        {
            const int offset = 1;
            const int critCount = 30;

            int top = _tracking.BrawlerTop.y;
            int bot = _tracking.BrawlerBottom.y;
            int x = _tracking.BrawlerBottom.x;

#if DEBUG
            using (var sub = (Bitmap)bmp.Clone(new Rectangle(x, top, 20, bot-top), bmp.PixelFormat))
            {
                for(int i=0;i<bot-top;i++)
                {
                    if (Is((0, i), IsBrawlerRed, sub))
                        sub.SetPixel(1, i, Color.Red);
                    else if (Is((0, i), IsBrawlerBlue, sub))
                        sub.SetPixel(1, i, Color.Blue);
                    else sub.SetPixel(1, i, Color.White);
                }
                sub.Save($"brawler\\brawler{brawlerCounter++}.bmp");
            }
#endif

            // down to up
            bool isRed = false;
            int count = 0;
            for (int y = bot; y > top; y -= offset)
            {
                if (Is((x, y), IsBrawlerRed, bmp))
                {
                    if (isRed)
                    {
                        count ++;
                        if (count >= critCount)
                        {
                            Console.WriteLine("B RIGHT");
                            return (false, true);
                        }
                    }
                    else
                    {
                        count = 1;
                        isRed = true;
                    }
                }
                else if (Is((x, y), IsBrawlerBlue, bmp))
                {
                    if (!isRed)
                    {
                        count ++;
                        if (count >= critCount)
                        {
                            Console.WriteLine("B LEFT");
                            return (true, false);
                        }
                    }
                    else
                    {
                        count = 1;
                        isRed = false;
                    }
                }
            }
            // Console.WriteLine($"END {(isRed ? "Red" : "Blue")} {count}");

            // Console.WriteLine("NOTHING");
            return (false, false);
        }

        private static bool Is((int x, int y) point, Func<Color, bool> isNeeded, Bitmap bmp)
            => isNeeded(bmp.GetPixel(point.x, point.y));

        private static bool IsRed(Color color) => color.R > 230;

        private static bool IsBlue(Color color) => color.B > 230;

        private static bool IsBrawlerRed(Color color) 
            => color.R > color.B && color.R > color.G;

        private static bool IsBrawlerBlue(Color color) 
            => color.B > color.R && color.B > color.G && color.B > 70;

        private static bool IsGrey(Color color) => color.R < 110 && color.B < 110 && color.G < 110;
    }
}