using HarmonyLib;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Monsters;

using VisitMountVapius.Models;

namespace VisitMountVapius.HarmonyPatches;

[HarmonyPatch(typeof(GameLocation))]
internal static class MonsterDropPatches
{
    internal const string MonsterKey = "VMV.Monster";

    [HarmonyPatch(nameof(GameLocation.monsterDrop))]
    private static void Postfix(GameLocation __instance, Monster monster, int x, int y, Farmer who)
    {
        if (ModEntry.ActiveLocation?.MonsterDropZones is not { } zones || zones.Count == 0)
        {
            return;
        }

        ModEntry.ModMonitor.VerboseLog($"Checking drops for {monster.Name}.");

        ItemQueryContext context = new(__instance, who, Random.Shared);
        context.CustomFields ??= new();
        context.CustomFields[MonsterKey] = monster;

        GameStateQueryContext queryContext = new(
            __instance,
            who,
            null,
            null,
            Random.Shared,
            null,
            context.CustomFields);

        foreach (LocationMonsterDrop zone in zones)
        {
            if (!zone.Contains(who.TilePoint))
            {
                continue;
            }

            if (zone.Items is null)
            {
                continue;
            }

            int repeat = who.isWearingRing("526") ? 2 : 1;
            List<Debris> debrisToAdd = new();
            for (int i = 0; i < repeat; i++)
            {

                if (!zone.CheckCondition(__instance, who, context.CustomFields))
                {
                    continue;
                }

                foreach (GeneralItemSpawnData item in zone.Items)
                {
                    if (!GameStateQuery.CheckConditions(item.Condition, queryContext))
                    {
                        continue;
                    }
                    IList<ItemQueryResult> list = ItemQueryResolver.TryResolve(
                        item,
                        context,
                        item.ItemQuerySearchMode,
                        avoidRepeat: true,
                        logError: (string query, string message) =>
                        {
                            ModEntry.ModMonitor.LogOnce($"Failed parsing item query '{query}' for zone '{zone.Id}'");
                        });

                    foreach (ItemQueryResult drop in list)
                    {
                        if (drop.Item is Item real)
                        {
                            debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(
                                item: real,
                                debrisOrigin: new(x, y),
                                targetLocation: who.StandingPixel.ToVector2())));

                        }
                    }
                }
            }

            foreach (var drop in debrisToAdd)
            {
                __instance.debris.Add(drop);
            }

            if (who.stats.Get("Book_Void") != 0 && Random.Shared.NextBool(0.03))
            {
                foreach (var debri in debrisToAdd)
                {
                    var newItem = debri.item?.getOne() as Item;
                    if (newItem is not null)
                    {
                        newItem.Stack = debri.item?.Stack ?? 1;
                        __instance.debris.Add(
                            monster.ModifyMonsterLoot(new Debris(
                                item: newItem,
                                debrisOrigin: new(x, y),
                                targetLocation: who.StandingPixel.ToVector2()))
                            );
                    }
                }
            }
        }
    }
}
