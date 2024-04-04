using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

using VisitMountVapius.HarmonyPatches;

using static StardewValley.GameStateQuery;

namespace VisitMountVapius.Framework;
internal static class MonsterGSQ
{
    internal static bool MaxHealth(string[] query, GameStateQueryContext context)
    {
        if (context.CustomFields?.TryGetValue(MonsterDropPatches.MonsterKey, out var obj) is not true || obj is not Monster monster)
        {
            return false;
        }

        if (!ArgUtility.TryGetInt(query, 1, out var minValue, out string error)
            ||  ArgUtility.TryGetOptionalInt(query,2, out var maxValue, out error, int.MaxValue))
        {
            return Helpers.ErrorResult(query, error);
        }

        return monster.MaxHealth >= minValue && monster.MaxHealth <= maxValue;
    }

    internal static bool Name(string[] query, GameStateQueryContext context)
    {
        if (context.CustomFields?.TryGetValue(MonsterDropPatches.MonsterKey, out var obj) is not true || obj is not Monster monster)
        {
            return false;
        }

        if (!ArgUtility.TryGet(query, 1, out var name, out var error))
        {
            return Helpers.ErrorResult(query, error);
        }

        for (int i = 1; i < query.Length; i++)
        {
            if (query[i] == monster.Name)
            {
                return true;
            }
        }

        return false;
    }
}
