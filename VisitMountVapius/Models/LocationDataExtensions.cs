namespace VisitMountVapius.Models;

internal sealed class LocationDataExtensions
{
    /// <summary>
    /// A list of zones additional artifact spots can spawn.
    /// </summary>
    public List<ArtifactSpotSpawnZone>? ArtifactSpawnZones { get; set; }

    /// <summary>
    /// A list of zones where additional monster drops will be added.
    /// </summary>
    public List<LocationMonsterDrop>? MonsterDropZones { get; set; }

    /// <summary>
    /// A list of zones where additional fish pan spots can happen.
    /// </summary>
    public List<EntryWithRectangle> FishSpotSpawnZones { get; set; }
    public List<EntryWithRectangle> PanSpotSpawnZones { get; set; }
}
