using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Tools;

using VisitMountVapius.Models;

namespace VisitMountVapius.Framework;
internal static class PanningAndFishingSpotManager
{
    private static readonly Dictionary<string, int> _lastUpdatedTime = new();

    internal static void Apply(GameLocation currentLocation, Farmer who)
    {
        if (currentLocation is null || ModEntry.ActiveLocation is not { } data)
        {
            return;
        }

        // check to see if we've run on this time interval yet.
        ref int time = ref CollectionsMarshal.GetValueRefOrAddDefault(_lastUpdatedTime, currentLocation.NameOrUniqueName, out bool exists);
        if (exists && time >= Game1.timeOfDay)
        {
            return;
        }
        time = Game1.timeOfDay;

        HandleFishSpots(currentLocation, data, who);
        HandlePanSpots(currentLocation, data, who);
    }

    internal static void Reset() => _lastUpdatedTime.Clear();

    private static void HandleFishSpots(GameLocation currentLocation, LocationDataExtensions data, Farmer who)
    {
        if (data.FishSpotSpawnZones?.Count is not > 0 || currentLocation.fishSplashPoint.Value != Point.Zero)
        {
            return;
        }

        foreach (var spawn in data.FishSpotSpawnZones)
        {
            if (!spawn.CheckCondition(currentLocation, who))
            {
                continue;
            }

            ModEntry.ModMonitor.VerboseLog($"Checking fish spawn data for {currentLocation.NameOrUniqueName}");

            Rectangle zone = spawn.Area.ClampMap(currentLocation);
            for (int tries = 0; tries < 8; tries ++)
            {
                Point p = zone.GetRandomTile();
                if (!currentLocation.isOpenWater(p.X, p.Y) || currentLocation.doesTileHaveProperty(p.X, p.Y, "NoFishing", "Back") is not null)
                {
                    continue;
                }
                int toLand = FishingRod.distanceToLand(p.X, p.Y, currentLocation);
                if (toLand > 1 && toLand < 5)
                {
                    if (Game1.player.currentLocation == currentLocation)
                    {
                        currentLocation.playSound("waterSlosh");
                    }
                    currentLocation.fishSplashPoint.Value = p;
                    return;
                }
            }
        }

    }

    private static void HandlePanSpots(GameLocation currentLocation, LocationDataExtensions data, Farmer who)
    {
        if (data.PanSpotSpawnZones?.Count is not > 0 || currentLocation.orePanPoint.Value != Point.Zero || !Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
        {
            return;
        }

        ModEntry.ModMonitor.VerboseLog($"Checking pan spawn data for {currentLocation.NameOrUniqueName}");

        foreach (EntryWithRectangle spawn in data.PanSpotSpawnZones)
        {
            if (!spawn.CheckCondition(currentLocation, who))
            {
                continue;
            }

            Rectangle zone = spawn.Area.ClampMap(currentLocation);
            for (int tries = 0; tries < 8; tries++)
            {
                Point p = zone.GetRandomTile();
                if (currentLocation.isOpenWater(p.X, p.Y) && currentLocation.getTileIndexAt(p, "Buildings") == -1
                    && FishingRod.distanceToLand(p.X, p.Y, currentLocation, true) <= 1)
                {
                    if (Game1.player.currentLocation == currentLocation)
                    {
                        currentLocation.playSound("slosh");
                    }
                    currentLocation.orePanPoint.Value = p;
                    return;
                }
            }
        }
    }
}
