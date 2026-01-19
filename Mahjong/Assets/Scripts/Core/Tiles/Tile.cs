namespace MJ.Core.Tiles
{
    public class Tile
    {
        public TileType tileType { get; private set; }

        public Tile(TileType tileType) {
            this.tileType = tileType;
        }
    }
}
