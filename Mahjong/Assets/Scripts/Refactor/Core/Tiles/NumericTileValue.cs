using System;

namespace MJ2.Core.Tiles
{
    /// <summary>
    /// Numeric tile value (1-9 for suited tiles, 1-4 for bonus tiles)
    /// </summary>
    public class NumericTileValue : TileValue
    {
        public int Number { get; }

        public NumericTileValue(int number)
        {
            if (number < 1 || number > 9)
                throw new ArgumentException($"Number must be between 1-9, got {number}");
            
            Number = number;
        }

        public override int NumericValue => Number;
        public override bool CanFormSequence => true;

        public override bool Equals(TileValue other)
        {
            return other is NumericTileValue numeric && numeric.Number == this.Number;
        }

        public override bool IsOneGreaterThan(TileValue other)
        {
            return other is NumericTileValue numeric && this.Number == numeric.Number + 1;
        }

        public override string ToString() => Number.ToString();
        public override string ToCompactString() => Number.ToString();

        public override int GetHashCode() => Number.GetHashCode();
        public override bool Equals(object obj) => obj is NumericTileValue other && Equals(other);
    }
}
