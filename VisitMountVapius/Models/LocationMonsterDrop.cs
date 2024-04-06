namespace VisitMountVapius.Models;
internal sealed class LocationMonsterDrop: EntryWithRectangle
{
    public List<GeneralItemSpawnData>? Items { get; set; } = null;

    public string Id { get; set; } = "Default";
}
