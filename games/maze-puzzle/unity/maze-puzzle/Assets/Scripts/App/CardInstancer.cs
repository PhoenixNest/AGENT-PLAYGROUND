namespace OrbitalSalvage.App
{
    /// <summary>CSS-pixel box. Card lists on these pages are a fixed grid: one measured origin, a constant gap, and each card's own height.</summary>
    public struct CardRect
    {
        public double X, Y, W, H;
        public CardRect(double x, double y, double w, double h) { X = x; Y = y; W = w; H = h; }
    }

    /// <summary>
    /// Places card sub-templates on the grid the web uses.
    /// Shop cards, trophy cards, and daily tier cards are a column. Level tiles are two columns; a row is as tall as its taller tile.
    /// </summary>
    public static class CardInstancer
    {
        public const double ShopX = 16;
        public const double ShopY = 293.5;
        public const double ShopW = 398;
        public const double ShopGap = 12;

        public const double TrophyX = 16;
        public const double TrophyY = 86;
        public const double TrophyW = 398;
        public const double TrophyGap = 12;

        public const double DailyX = 16;
        public const double DailyW = 398;
        public const double DailyGap = 14;

        public const double TileX0 = 16;
        public const double TileX1 = 220;
        public const double TileY = 223.188;
        public const double TileW = 194;
        public const double TileRowGap = 10;

        public static CardRect[] Stack(double x, double y0, double width, double gap, double[] heights)
        {
            var rects = new CardRect[heights.Length];
            double y = y0;
            for (int i = 0; i < heights.Length; i++)
            {
                rects[i] = new CardRect(x, y, width, heights[i]);
                y += heights[i] + gap;
            }
            return rects;
        }

        public static CardRect[] StackShop(double[] heights) => Stack(ShopX, ShopY, ShopW, ShopGap, heights);

        public static CardRect[] StackTrophies(double[] heights) => Stack(TrophyX, TrophyY, TrophyW, TrophyGap, heights);

        public static CardRect[] StackDaily(double y0, double[] heights) => Stack(DailyX, y0, DailyW, DailyGap, heights);

        /// <summary>Two-column level grid. <paramref name="heights"/> is row-major, left then right.</summary>
        public static CardRect[] LevelGrid(double[] heights)
        {
            var rects = new CardRect[heights.Length];
            double y = TileY;
            for (int i = 0; i < heights.Length; i += 2)
            {
                double row = heights[i];
                if (i + 1 < heights.Length && heights[i + 1] > row) row = heights[i + 1];
                rects[i] = new CardRect(TileX0, y, TileW, heights[i]);
                if (i + 1 < heights.Length) rects[i + 1] = new CardRect(TileX1, y, TileW, heights[i + 1]);
                y += row + TileRowGap;
            }
            return rects;
        }

        /// <summary>Move <paramref name="rect"/> by the same delta that takes the template root onto <paramref name="slot"/>.</summary>
        public static CardRect Place(CardRect slot, double originX, double originY, double x, double y, double w, double h)
        {
            return new CardRect(x + (slot.X - originX), y + (slot.Y - originY), w, h);
        }
    }
}
