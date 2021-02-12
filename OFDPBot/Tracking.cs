namespace OFDPBot
{
    public class Tracking
    {
        public Tracking(
            (int, int) left1,
            (int, int) left2,
            (int, int) right1,
            (int, int) right2,
            (int, int) brawlerTop,
            (int, int) brawlerBottom,
            (int, int) leftHealth)
        {
            Left1 = left1;
            Left2 = left2;
            Right1 = right1;
            Right2 = right2;
            BrawlerTop = brawlerTop;
            BrawlerBottom = brawlerBottom;
            LeftHealth = leftHealth;
        }

        public (int x, int y) Left1 { get; }

        public (int x, int y) Left2 { get; }

        public (int x, int y) Right1 { get; }

        public (int x, int y) Right2 { get; }

        public (int x, int y) BrawlerTop { get; }

        public (int x, int y) BrawlerBottom { get; }
        public (int, int) LeftHealth { get; }

        public override string ToString()
            => $"LEFT {Left1} {Left2}\nRIGHT {Right1} {Right2}\nBrawler Top {BrawlerTop} Bot {BrawlerBottom}"
             + $"\nLEFT HEALTH {LeftHealth}";
    }
}