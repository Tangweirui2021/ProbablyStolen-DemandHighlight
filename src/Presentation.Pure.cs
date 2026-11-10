// Fork presentation rules. No Unity/game references: tested without a copy of the game.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DemandHighlight
{
    [Flags]
    public enum Risk : byte { None = 0, Stolen = 1, Contraband = 2, Both = 3 }

    public struct MarkerColor
    {
        public byte R, G, B, A;
        public MarkerColor(byte r, byte g, byte b, byte a = 255) { R = r; G = g; B = b; A = a; }
        public string Hex { get { return "#" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2") + A.ToString("X2"); } }

        public static bool TryParse(string text, out MarkerColor color)
        {
            color = default(MarkerColor);
            if (text == null) return false;
            text = text.Trim();
            if (text.StartsWith("#", StringComparison.Ordinal)) text = text.Substring(1);
            if (text.Length != 6 && text.Length != 8) return false;
            uint value;
            if (!uint.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value)) return false;
            if (text.Length == 6) value = (value << 8) | 255u;
            color = new MarkerColor((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
            return true;
        }
    }

    public static class Presentation
    {
        // Independent bits are essential: upstream's Mark deliberately loses "also stolen" for contraband.
        public static Risk RiskOf(bool stolen, bool contraband)
        {
            return (stolen ? Risk.Stolen : Risk.None) | (contraband ? Risk.Contraband : Risk.None);
        }

        public static Style DemandStyle(Style upstream)
        {
            return new Style(upstream.Mark == Mark.None ? Mark.None : Mark.Normal, upstream.Contents);
        }

        // Inspection/seller illegality is drawn by RiskStars, never by the demand-outline engine.
        public static Query DemandQuery(ClientView client, FilterView filter)
        {
            if (client != null && client.Intent == Intent.Inspection) client = null;
            return Intent.Distill(client, filter, false);
        }

        // These are legal-status classifications, not product categories. Never let them choose a demand colour.
        public static bool ProductType(string type)
        {
            return !string.IsNullOrEmpty(type) &&
                !type.Equals("CONTRABAND", StringComparison.OrdinalIgnoreCase) &&
                !type.Equals("STOLEN", StringComparison.OrdinalIgnoreCase) &&
                !type.StartsWith("RESTRICTED_", StringComparison.OrdinalIgnoreCase);
        }

        // First configured priority wins. Unknown types use a stable ordinal fallback, never list iteration order.
        public static string SelectType(IList<string> types, IList<string> priority)
        {
            if (types == null) return null;
            if (priority != null)
                for (int p = 0; p < priority.Count; p++)
                    if (ProductType(priority[p]))
                        for (int t = 0; t < types.Count; t++)
                            if (string.Equals(priority[p], types[t], StringComparison.OrdinalIgnoreCase)) return types[t];
            string best = null;
            foreach (var type in types)
                if (ProductType(type) && (best == null || StringComparer.OrdinalIgnoreCase.Compare(type, best) < 0)) best = type;
            return best;
        }

        // Level 2 stays close to one third of a single cell, independently of the item's footprint.
        // Keep the saved 1..4 setting from wrt.2, but shrink its interpretation for existing installations too.
        public static int StarSize(int gridSize, int level)
        {
            if (gridSize <= 0) gridSize = 16;
            level = Math.Max(1, Math.Min(4, level));
            return Math.Max(1, (int)Math.Round(gridSize * level / 6.0, MidpointRounding.AwayFromZero));
        }

        // White alpha masks, including one pixel of transparent padding for a configurable dark outline.
        // A tiny five-point star, not a font glyph (which may be absent in some game languages).
        public const int StarSide = 11;
        static readonly string[] StarRows = {
            // Two rows of horizontal arms keep the five-point silhouette when point-sampled to 5 UI pixels.
            "....#....", "...###...", "#########", "#########", ".#######.",
            "..#####..", "..#####..", ".###.###.", ".##...##."
        };

        public static byte[] StarMask(bool outline)
        {
            var fill = new byte[StarSide * StarSide];
            for (int y = 0; y < 9; y++) for (int x = 0; x < 9; x++)
                if (StarRows[y][x] == '#') fill[(y + 1) * StarSide + x + 1] = 255;
            if (!outline) return fill;
            var edge = new byte[fill.Length];
            for (int y = 0; y < StarSide; y++) for (int x = 0; x < StarSide; x++)
            {
                if (fill[y * StarSide + x] != 0) continue;
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                {
                    int xx = x + dx, yy = y + dy;
                    if (xx >= 0 && xx < StarSide && yy >= 0 && yy < StarSide && fill[yy * StarSide + xx] != 0)
                        edge[y * StarSide + x] = 255;
                }
            }
            return edge;
        }
    }
}
