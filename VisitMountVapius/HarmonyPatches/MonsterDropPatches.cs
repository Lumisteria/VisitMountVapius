using HarmonyLib;

using StardewValley;
using StardewValley.Internal;
using StardewValley.Monsters;

namespace VisitMountVapius.HarmonyPatches;

[HarmonyPatch(typeof(GameLocation))]
internal static class MonsterDropPatches
{
    internal const string MonsterKey = "VMV.Monster";

    private static void Postfix(GameLocation __instance, Monster monster, Farmer who)
    {
        if (ModEntry.ActiveLocation?.MonsterDropZones is not { } zones)
        {
            return;
        }

        ItemQueryContext context = new(__instance, who, Random.Shared);
        context.CustomFields ??= new();
        context.CustomFields[MonsterKey] = monster;

        foreach (var zone in zones)
        {
            if (!zone.CheckCondition(__instance,who))
            {
                continue;
            }

            if (!zone.Contains(who.TilePoint))
            {
                continue;
            }

            if (zone.Items is null)
            {
                continue;
            }

            foreach (var item in zone.Items)
            {
                var list = ItemQueryResolver.TryResolve(
                    item,
                    context,
                    item.ItemQuerySearchMode,
                    avoidRepeat: true,
                    logError: (string query, string message) =>
                    {
                        ModEntry.ModMonitor.LogOnce($"Failed parsing item query '{query}' for zone '{zone.Id}'");
                    });
            }
        }
    }
}
