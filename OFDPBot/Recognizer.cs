using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OFDPBot
{
    internal class Recognizer : IDisposable
    {
        private readonly Tracking _tracking;
        private readonly Bitmap _redBrawlSample;
        private readonly Bitmap _blueBrawlSample;
        // private readonly BitmapData _redBrawlSampleData;
        // private readonly BitmapData _blueBrawlSampleData;
        private readonly Stopwatch _watch; 

        public Recognizer(Tracking tracking)
        {
            _tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
            _watch = new Stopwatch();

            _redBrawlSample = (Bitmap)Bitmap.FromFile("red_brawl_sample.bmp");
            // _redBrawlSampleData = ExtractData(_redBrawlSample);
            _blueBrawlSample = (Bitmap)Bitmap.FromFile("blue_brawl_sample.bmp");
            // _blueBrawlSampleData = ExtractData(_blueBrawlSample);
        }

        public void Dispose()
        {
            // _redBrawlSample.UnlockBits(_redBrawlSampleData);
            _redBrawlSample.Dispose();
            // _blueBrawlSample.UnlockBits(_blueBrawlSampleData);
            _blueBrawlSample.Dispose();
        }

        public (bool, bool) Recognize(Bitmap bmp, out bool isBrawler)
        {
            _watch.Restart();
            isBrawler = false;
            (bool, bool) move = (false, false);
            if (!Is(_tracking.LeftHealth, IsRed, bmp))
            {
                Console.WriteLine("BRAWLER MODE");
                move = CheckBrawlerWithPatternMatching(bmp);
                if (move.Item1)
                {
                    isBrawler = true;
                    Console.Write("B LEFT");
                }
                else if (move.Item2)
                {
                    isBrawler = true;
                    Console.Write("B RIGHT");
                }
            }
            else if (Is(_tracking.Left1, IsBlue, bmp))
            {
                Console.Write("LEFT");
                move = (true, false);
            }
            else if (Is(_tracking.Right1, IsRed, bmp))
            {
                Console.Write("RIGHT");
                move = (false, true);
            }
            
            _watch.Stop();
            if (move.Item1 || move.Item2)
                Console.WriteLine(" in " + _watch.ElapsedMilliseconds + "ms");
            
            return move;
        }

        private static int brawlerCounter = 0;

        private (bool, bool) CheckBrawlerWithPatternMatching(Bitmap screen)
        {
            screen.RotateFlip(RotateFlipType.Rotate180FlipNone);
            // screen.Save($"brawler\\search{brawlerCounter++}.bmp");
            // _redBrawlSample.Save($"brawler\\red.bmp");
            // _blueBrawlSample.Save($"brawler\\blue.bmp");

            const double maxTolerance = 0.3;
            for(double tolerance = 0; tolerance < maxTolerance; tolerance += 0.1)
            {
                (var blue, var red) = Find(screen, tolerance);
                if (blue != Rectangle.Empty)
                    return (true, false);
                if (red != Rectangle.Empty)
                    return (false, true);
            }

            return (false, false);
        }

        private (Rectangle blue, Rectangle red) Find(Bitmap bitmap, double tolerance)
        {
            var red = SearchBitmap(_redBrawlSample, bitmap, tolerance);
            var blue = SearchBitmap(_blueBrawlSample, bitmap, tolerance);
            return (blue, red);
        }

        private static Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance)
        {
            BitmapData smallData = smallBmp.LockBits(new Rectangle(0, 0, smallBmp.Width, smallBmp.Height),
                       System.Drawing.Imaging.ImageLockMode.ReadOnly,
                       System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bigData = bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height),
                       System.Drawing.Imaging.ImageLockMode.ReadOnly,
                       System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int smallStride = smallData.Stride;
            int bigStride = bigData.Stride;

            int bigWidth = bigBmp.Width;
            int bigHeight = bigBmp.Height - smallBmp.Height + 1;
            int smallWidth = smallBmp.Width * 3;
            int smallHeight = smallBmp.Height;

            Rectangle location = Rectangle.Empty;
            int margin = Convert.ToInt32(255.0 * tolerance);

            unsafe
            {
                byte* pSmall = (byte*)(void*)smallData.Scan0;
                byte* pBig = (byte*)(void*)bigData.Scan0;

                int smallOffset = smallStride - smallBmp.Width * 3;
                int bigOffset = bigStride - bigBmp.Width * 3;

                bool matchFound = true;

                for (int y = 0; y < bigHeight; y++)
                {
                    for (int x = 0; x < bigWidth; x++)
                    {
                        byte* pBigBackup = pBig;
                        byte* pSmallBackup = pSmall;

                        //Look for the small picture.
                        for (int i = 0; i < smallHeight; i++)
                        {
                            int j = 0;
                            matchFound = true;
                            for (j = 0; j < smallWidth; j++)
                            {
                                //With tolerance: pSmall value should be between margins.
                                int inf = pBig[0] - margin;
                                int sup = pBig[0] + margin;
                                if (sup < pSmall[0] || inf > pSmall[0])
                                {
                                    matchFound = false;
                                    break;
                                }

                                pBig++;
                                pSmall++;
                            }

                            if (!matchFound) break;

                            //We restore the pointers.
                            pSmall = pSmallBackup;
                            pBig = pBigBackup;

                            //Next rows of the small and big pictures.
                            pSmall += smallStride * (1 + i);
                            pBig += bigStride * (1 + i);
                        }

                        //If match found, we return.
                        if (matchFound)
                        {
                            location.X = x;
                            location.Y = y;
                            location.Width = smallBmp.Width;
                            location.Height = smallBmp.Height;
                            break;
                        }
                        //If no match found, we restore the pointers and continue.
                        else
                        {
                            pBig = pBigBackup;
                            pSmall = pSmallBackup;
                            pBig += 3;
                        }
                    }

                    if (matchFound) break;

                    pBig += bigOffset;
                }
            }

            bigBmp.UnlockBits(bigData);
            smallBmp.UnlockBits(smallData);
            return location;
        }

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