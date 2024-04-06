using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

using VisitMountVapius.HarmonyPatches;

using static StardewValley.GameStateQuery;

namespace VisitMountVapius.Framework.GSQ;
internal static class MonsterGSQ
{
    internal static bool MaxHealth(string[] query, GameStateQueryContext context)
    {
        if (context.CustomFields?.TryGetValue(MonsterDropPatches.MonsterKey, out object? obj) is not true || obj is not Monster monster)
        {
            return false;
        }

        if (!ArgUtility.TryGetInt(query, 1, out int minValue, out string error)
            ||  ArgUtility.TryGetOptionalInt(query,2, out int maxValue, out error, int.MaxValue))
        {
            return Helpers.ErrorResult(query, error);
        }

        return monster.MaxHealth >= minValue && monster.MaxHealth <= maxValue;
    }

    internal static bool Name(string[] query, GameStateQueryContext context)
    {
        if (context.CustomFields?.TryGetValue(MonsterDropPatches.MonsterKey, out object? obj) is not true || obj is not Monster monster)
        {
            return false;
        }

        if (!ArgUtility.TryGet(query, 1, out string? name, out string? error))
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
