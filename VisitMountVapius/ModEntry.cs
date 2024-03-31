using HarmonyLib;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using VisitMountVapius.Framework;

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
    }
}
