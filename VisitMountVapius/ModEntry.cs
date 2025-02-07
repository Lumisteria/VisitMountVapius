﻿using HarmonyLib;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Internal;

using VisitMountVapius.Framework;
using VisitMountVapius.Framework.GSQ;
using VisitMountVapius.Framework.ItemQueries;
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

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

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

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        MissingArtifact.Reset();
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        GameLocation.RegisterTileAction("vmv.linked.chest", LinkedChest.Action);
        GameLocation.RegisterTouchAction("vmv.teleport.player", TeleportPlayer.ApplyCommand);

        GameStateQuery.Register("VMV.HasSeenActiveDialogueEvent", HasSeenActiveDialogueEvent.Query);
        GameStateQuery.Register("VMV.MONSTER_NAME", MonsterGSQ.Name);
        GameStateQuery.Register("VMV.MONSTER_MAX_HEALTH", MonsterGSQ.MaxHealth);

        ItemQueryResolver.Register("VMV.MISSING_ARTIFACT_OR_ITEM", MissingArtifact.Query);

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
        if (location is GameLocation current
            && (data.TryGetValue(current.Name, out LocationDataExtensions? locationData) ||
                data.TryGetValue(current.GetLocationContextId(), out locationData)))
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
        if (ReferenceEquals(e.OldLocation, e.NewLocation) || e.NewLocation is not { } loc)
        {
            return;
        }

        // fix wind effects sometimes persisting when it shouldn't.
        if (loc.IsDebrisWeatherHere())
        {
            if (!loc.ignoreDebrisWeather.Value && Game1.debrisWeather.Count == 0)
            {
                Game1.populateDebrisWeatherArray();
            }
        }
        else
        {
            Game1.debrisWeather.Clear();
        }

        SetLocationData(loc);
        PanningAndFishingSpotManager.Apply(loc, Game1.player);
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
