using System.Drawing;

namespace OFDPBot
{
    internal class Recognizer
    {
        private readonly (int x, int y) _left;
        private readonly (int x, int y) _right;

        public Recognizer((int, int) left, (int, int) right)
        {
            _left = left;
            _right = right;
        }

        public (bool, bool) Recognize(Bitmap bmp) 
            => (
                IsNeededColor(bmp.GetPixel(_left.x, _left.y)),
                IsNeededColor(bmp.GetPixel(_right.x, _right.y))
                );

        private bool IsNeededColor(Color color) => color.R > 230 && color.G > 230 && color.B > 230; //white
    }
}