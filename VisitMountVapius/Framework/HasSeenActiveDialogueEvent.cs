using StardewValley;
using StardewValley.Delegates;

namespace VisitMountVapius.Framework;

internal static class HasSeenActiveDialogueEvent
{
    internal static bool Query(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var playerKey, out var error) || !ArgUtility.TryGet(query, 2, out var dialogueTopic, out error))
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