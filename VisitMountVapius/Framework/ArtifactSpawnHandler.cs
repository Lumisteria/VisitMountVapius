using Microsoft.Xna.Framework;

using StardewModdingAPI;
using StardewValley;
using VisitMountVapius.Models;

namespace VisitMountVapius.Framework;

internal static class ArtifactSpawnHandler
{
    internal static void Spawn()
    {
        Dictionary<string, List<ArtifactSpotSpawnZone>>? _dataFromContexts = null;

        foreach ((string location, LocationDataExtensions data) in AssetLoader.GetLocationData())
        {
            if (data.ArtifactSpawnZones is { } zoneData && zoneData.Count > 0)
            {
                if (Game1.getLocationFromName(location) is GameLocation loc)
                {
                    HandleArtifactSpawning(loc, zoneData);
                    continue;
                }

                if (Game1.locationContextData.ContainsKey(location))
                {
                    _dataFromContexts ??= new();
                    _dataFromContexts[location] = zoneData;
                    continue;
                }

                ModEntry.ModMonitor.LogOnce($"Location extension data references {location} which does not seem to exist, skipping.", LogLevel.Warn);
                continue;
            }
        }

        // okay, now checking location context data
        if (_dataFromContexts is not null)
        {
            ModEntry.ModMonitor.Log($"Spawning artifact spots for location data: ");

            Utility.ForEachLocation(
                location =>
                {
                    string? context = location.GetLocationContextId();
                    if (context is not null && _dataFromContexts.TryGetValue(context, out List<ArtifactSpotSpawnZone>? data)
                        && AssetLoader.GetLocationData().GetValueOrDefault(location.NameOrUniqueName)?.ArtifactSpawnZones is null)
                    {
                        HandleArtifactSpawning(location, data);
                    }
                    return true;
                });
        }
    }

    private static void HandleArtifactSpawning(GameLocation loc, List<ArtifactSpotSpawnZone> zoneData)
    {
        ModEntry.ModMonitor.Log($"Handling artifact zone spawning for {loc.NameOrUniqueName}");
        try
        {
            foreach (ArtifactSpotSpawnZone zone in zoneData)
            {
                if (!zone.CheckCondition(loc, Game1.player))
                {
                    continue;
                }

                int count = zone.Range.Get();
                if (count <= 0)
                {
                    continue;
                }

                int safety = 50;
                Rectangle possible = zone.Area.ClampMap(loc);
                while (count > 0 && safety > 0)
                {
                    Point p = possible.GetRandomTile();
                    Vector2 v = p.ToVector2();

                    // same checks as game.
                    if (loc.CanItemBePlacedHere(v, true)
                        && !loc.IsTileOccupiedBy(v)
                        && loc.getTileIndexAt(p.X, p.Y, "AlwaysFront") == -1
                        && loc.getTileIndexAt(p.X, p.Y, "Front") == -1
                        && !loc.isBehindBush(v)
                        && (loc.doesTileHaveProperty(p.X, p.Y, "Diggable", "Back") is not null
                            || (loc.GetSeason() == Season.Winter && loc.doesTileHaveProperty(p.X, p.Y, "Type", "Back") == "Grass")))
                    {
                        if (loc.Objects.TryAdd(v, ItemRegistry.Create<SObject>(zone.Type)))
                        {
                            count--;
                        }
                    }

                    safety--;
                }
            }
        }
        catch (Exception ex)
        {
            ModEntry.ModMonitor.LogError($"spawning artifact spots for {loc.NameOrUniqueName}", ex);
        }
    }
}