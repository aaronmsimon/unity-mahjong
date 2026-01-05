using System.Collections.Generic;

namespace MJ2.Core.Tiles
{
    public class TileSetFactory
    {
        private TileSetSO tileSetSO;

        public TileSetFactory(TileSetSO tileSetSO) {
            this.tileSetSO = tileSetSO;
        }

        public List<Tile> CreateTileSest() {
            return tileSetSO.CreateTileSet();
        }
    }
}
