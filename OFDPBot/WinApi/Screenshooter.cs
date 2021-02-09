using System.Drawing;
using System.Drawing.Imaging;

namespace OFDPBot.WinApi
{
    internal class Screenshooter
    {
        private readonly Rect _rect;

        public Screenshooter(Rect rect)
        {
            _rect = rect;
        }

        public Bitmap MakeScreenshot()
        {
            int width = _rect.Right - _rect.Left;
            int height = _rect.Bottom - _rect.Top;
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(_rect.Left, _rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }
    }
}