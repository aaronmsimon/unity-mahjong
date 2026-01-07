using System;

namespace MJ2.Core.Tiles
{
    /// <summary>
    /// Dragon tile value
    /// </summary>
    public class DragonTileValue : TileValue
    {
        public DragonType Dragon { get; }

        public DragonTileValue(DragonType dragon)
        {
            Dragon = dragon;
        }

        public override int NumericValue => -1;
        public override bool CanFormSequence => false;

        public override bool Equals(TileValue other)
        {
            return other is DragonTileValue dragon && dragon.Dragon == this.Dragon;
        }

        public override bool IsOneGreaterThan(TileValue other) => false;

        public override string ToString() => $"{Dragon}";
        public override string ToCompactString() => Dragon switch
        {
            DragonType.Red => "RD",
            DragonType.Green => "GD",
            DragonType.White => "WD",
            _ => "?D"
        };

        public override int GetHashCode() => Dragon.GetHashCode();
        public override bool Equals(object obj) => obj is DragonTileValue other && Equals(other);
    }
}
