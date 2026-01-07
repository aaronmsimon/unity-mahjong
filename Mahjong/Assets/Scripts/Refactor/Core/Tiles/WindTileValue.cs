using System;

namespace MJ2.Core.Tiles
{
    /// <summary>
    /// Wind tile value
    /// </summary>
    public class WindTileValue : TileValue
    {
        public WindType Wind { get; }

        public WindTileValue(WindType wind)
        {
            Wind = wind;
        }

        public override int NumericValue => -1;
        public override bool CanFormSequence => false;

        public override bool Equals(TileValue other)
        {
            return other is WindTileValue wind && wind.Wind == this.Wind;
        }

        public override bool IsOneGreaterThan(TileValue other) => false;

        public override string ToString() => $"{Wind}";
        public override string ToCompactString() => Wind switch
        {
            WindType.East => "EW",
            WindType.South => "SW",
            WindType.West => "WW",
            WindType.North => "NW",
            _ => "?W"
        };

        public override int GetHashCode() => Wind.GetHashCode();
        public override bool Equals(object obj) => obj is WindTileValue other && Equals(other);
    }
}
