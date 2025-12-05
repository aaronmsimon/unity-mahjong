public class TileInstance
{
    public TileType Type { get; }
    public int InstanceId { get; }

    public TileInstance(TileType type, int instanceId)
    {
        Type = type;
        InstanceId = instanceId;
    }

    public override string ToString()
    {
        return $"#{InstanceId}: {Type}";
    }
}
