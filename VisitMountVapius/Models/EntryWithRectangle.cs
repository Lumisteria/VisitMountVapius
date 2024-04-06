using Microsoft.Xna.Framework;

namespace VisitMountVapius.Models;

/// <summary>
/// A data entry that has a rectangle area.
/// </summary>
internal class EntryWithRectangle: BaseEntry
{
    /// <summary>
    /// The rectangular area to check. Defaults to the whole map.
    /// </summary>
    public Rectangle Area { get; set; } = new(0, 0, -1, -1);

    /// <summary>Checks to see if the point exists within the rectangle.</summary>
    internal bool Contains(Point point)
    {
        if (point.X < this.Area.X || point.Y < this.Area.Y)
        {
            return false;
        }
        if (this.Area.Height != -1 && point.X > this.Area.Right)
        {
            return false;
        }
        if (this.Area.Width != -1 && point.Y > this.Area.Bottom)
        {
            return false;
        }

        return true;
    }
}
