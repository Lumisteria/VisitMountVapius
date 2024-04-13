using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;

using VisitMountVapius.Framework;
using VisitMountVapius.Framework.GSQ;
using VisitMountVapius.Interfaces;
using VisitMountVapius.Models;

namespace VisitMountVapius;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    internal static IMonitor ModMonitor = null!;

    private static readonly PerScreen<LocationDataExtensions?> _activeLocation = new();

    internal static LocationDataExtensions? ActiveLocation
    {
        get => _activeLocation.Value;
        private set => _activeLocation.Value = value;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModMonitor = this.Monitor;

        Harmony harmony = new(helper.ModRegistry.ModID);

        harmony.PatchAll(typeof(ModEntry).Assembly);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Player.Warped += this.OnPlayerWarped;
        helper.Events.GameLoop.DayStarted += this.OnDayStart;

        helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;

        AssetLoader.Init(helper.GameContent);
        helper.Events.Content.AssetRequested += static (_, e) => AssetLoader.Load(e);

#if DEBUG
        helper.ConsoleCommands.Add("launch_test_chest", "test_chest", (arg, args) =>
        {
            LinkedChest.Action(Game1.getFarm(), new[] { "vmv.linked.chest", "test-chest" }, Game1.player, Game1.player.TilePoint);
        });
#endif
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        GameLocation.RegisterTileAction("vmv.linked.chest", LinkedChest.Action);

        GameStateQuery.Register("VMV.HasSeenActiveDialogueEvent", HasSeenActiveDialogueEvent.Query);
        GameStateQuery.Register("VMV.MONSTER_NAME", MonsterGSQ.Name);
        GameStateQuery.Register("VMV.MONSTER_MAX_HEALTH", MonsterGSQ.MaxHealth);

        // handle integration with events tester.
        try
        {
            IEventTesterAPI? api = this.Helper.ModRegistry.GetApi<IEventTesterAPI>("sinZandAtravita.SinZsEventTester");
            api?.RegisterAsset(AssetLoader.LocationExtensions);
        }
        catch (Exception ex)
        {
            ModMonitor.LogError("mapping Event Tester's API", ex);
        }
    }

    private static void SetLocationData(GameLocation location)
    {
        Dictionary<string, LocationDataExtensions> data = AssetLoader.GetLocationData();
        if (location is { } current
            && (data.TryGetValue(current.Name, out LocationDataExtensions? locationData) ||
                data.TryGetValue(current.locationContextId, out locationData)))
        {
            ActiveLocation = locationData;
        }
        else
        {
            ActiveLocation = null;
        }
    }

    private void OnPlayerWarped(object? sender, WarpedEventArgs e)
    {
        if (ReferenceEquals(e.OldLocation, e.NewLocation))
        {
            return;
        }

        SetLocationData(e.NewLocation);
        PanningAndFishingSpotManager.Apply(e.NewLocation, Game1.player);
    }

    private void OnDayStart(object? sender, DayStartedEventArgs e)
    {
        SetLocationData(Game1.currentLocation);
        PanningAndFishingSpotManager.Reset();

        if (!Context.IsMainPlayer)
        {
            return;
        }

        ArtifactSpawnHandler.Spawn();
    }

    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        PanningAndFishingSpotManager.Apply(Game1.currentLocation, Game1.player);
    }
}
