namespace MJ.Core.Tiles
{
    public class TileInstance
    {
        public int InstanceID { get; }
        public TileDefinitionSO Definition { get; }

        public TileID ID => Definition.TileInfo;

        public bool IsFaceUp { get; private set; }

        public TileInstance(int instanceID, TileDefinitionSO definition) {
            InstanceID = instanceID;
            Definition = definition;
        }

        public void FlipTile() {
            IsFaceUp = !IsFaceUp;
        }

        public override string ToString() => $"#{InstanceID} {ID}";
    }
}
