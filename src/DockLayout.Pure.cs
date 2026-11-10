using System;

namespace DemandHighlight
{
    // Coordinates are in the inventory window parent's local UI units, independent of screen resolution.
    public struct DockBounds
    {
        public float Right, Bottom, Width, Height;
        public bool Visible { get { return Width >= 60f && Height >= 28f; } }
        public int Columns { get { return Width >= 90f ? 2 : 1; } }
    }

    public static class DockLayout
    {
        public static DockBounds Fit(float inventoryLeft, float inventoryBottom, float canvasLeft, float canvasBottom,
            bool hasTrash, float trashLeft, float trashBottom)
        {
            float right = inventoryLeft - 4f;
            float bottom = Math.Max(inventoryBottom, canvasBottom + 4f);
            float left = hasTrash ? Math.Max(canvasLeft + 4f, trashLeft - 8f) : canvasLeft + 4f;
            return new DockBounds {
                Right = right, Bottom = bottom,
                Width = Math.Max(0f, Math.Min(100f, right - left)),
                Height = Math.Max(0f, Math.Min(56f, hasTrash ? trashBottom - bottom - 4f : 56f)),
            };
        }
    }
}
