using StardewValley.GameData;

namespace VisitMountVapius.Models;
internal sealed class LocationMonsterDrop: EntryWithRectangle
{
    public List<GeneralItemSpawnData>? Items { get; set; } = null;

    string ID { get; set; } = "Default";
}
