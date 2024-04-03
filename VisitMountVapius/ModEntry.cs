using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using VisitMountVapius.Framework;
using VisitMountVapius.Models;

namespace VisitMountVapius;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    internal static IMonitor ModMonitor = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModMonitor = this.Monitor;

        Harmony harmony = new(helper.ModRegistry.ModID);

        harmony.PatchAll(typeof(ModEntry).Assembly);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStart;

        AssetLoader.Init(helper.GameContent);
        helper.Events.Content.AssetRequested += static (_, e) => AssetLoader.Load(e);

#if DEBUG
        helper.ConsoleCommands.Add("launch_test_chest", "test_chest", (arg, args) =>
        {
            LinkedChest.Action(Game1.getFarm(), new[] { "vmv.linked.chest", "test-chest" }, Game1.player, Game1.player.TilePoint);
        });
#endif
    }

    private void OnDayStart(object? sender, DayStartedEventArgs e)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }

        foreach ((string location, LocationDataExtensions data) in AssetLoader.GetLocationData())
        {
            if (Game1.getLocationFromName(location) is not GameLocation loc)
            {
                this.Monitor.LogOnce($"Location extension data references {location} which does not seem to exist, skipping.", LogLevel.Warn);
                continue;
            }

            if (data.ArtifactSpawnZones?.Count is > 0)
            {
                foreach (ArtifactSpotSpawnZone zone in data.ArtifactSpawnZones)
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
                            loc.Objects.TryAdd(v, ItemRegistry.Create<SObject>(zone.Type));
                        }
                    }
                }
            }    
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        GameLocation.RegisterTileAction("vmv.linked.chest", LinkedChest.Action);

        GameStateQuery.Register("VMV.HasSeenActiveDialogueEvent", HasSeenActiveDialogueEvent.Query);
    }
}
