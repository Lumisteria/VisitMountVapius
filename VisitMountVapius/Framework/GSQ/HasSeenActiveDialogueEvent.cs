using StardewValley;
using StardewValley.Delegates;

namespace VisitMountVapius.Framework.GSQ;

internal static class HasSeenActiveDialogueEvent
{
    internal static bool Query(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out string? playerKey, out string? error) || !ArgUtility.TryGet(query, 2, out string? dialogueTopic, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        return GameStateQuery.Helpers.WithPlayer(
            context.Player,
            playerKey,
            farmer =>
            {
                for (int i = 2; i < query.Length; i++)
                {
                    if (farmer.hasSeenActiveDialogueEvent(query[i]))
                    {
                        return true;
                    }
                }
                return false;
            });

    }
}