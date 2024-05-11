using StardewModdingAPI;

using StardewValley;
using StardewValley.Internal;
using StardewValley.Locations;

using static StardewValley.Internal.ItemQueryResolver;

namespace VisitMountVapius.Framework.ItemQueries;
internal static class MissingArtifact
{
    /// <summary>
    /// A list of qualified item IDs of artifacts we are missing.
    /// </summary>
    private static List<string>? _missingArtifacts = new();

    internal static void Reset() => _missingArtifacts = null;

    /// <inheritdoc cref="StardewValley.Delegates.ResolveItemQueryDelegate"/>
    internal static IEnumerable<ItemQueryResult> Query(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
    {
        Populate();

        if (_missingArtifacts?.Count is > 0)
        {
            bool foundItem = false;

            for (int i = _missingArtifacts.Count - 1; i >= 0; i--)
            {
                string item = _missingArtifacts[i];
                if (!LibraryMuseum.IsItemSuitableForDonation(item, true))
                {
                    _missingArtifacts.RemoveAt(i);
                }

                if (avoidItemIds?.Contains(item) == true)
                {
                    continue;
                }

                Item? obj = null;
                try
                {
                    obj = ItemRegistry.Create(item, allowNull: true);
                }
                catch (Exception ex)
                {
                    Helpers.ErrorResult(key, arguments, logError, "creating item for missing artifact query, see log for details.");
                    ModEntry.ModMonitor.Log(ex.ToString());
                }

                if (obj is not null)
                {
                    yield return new(obj);
                    foundItem = true;
                }
            }

            if (foundItem)
            {
                yield break;
            }
        }

        // failed!
        if (string.IsNullOrWhiteSpace(arguments))
        {
            yield break;
        }
        ItemQueryResult[] items = ItemQueryResolver.TryResolve(arguments, new ItemQueryContext(context));
        foreach (ItemQueryResult? forwarded in items)
        {
            yield return forwarded;
        }
    }

    private static void Populate()
    {
        if (_missingArtifacts is not null)
        {
            return;
        }

        if (Game1.getLocationFromName("ArchaeologyHouse") is not LibraryMuseum musem)
        {
            ModEntry.ModMonitor.Log($"Could not populate - museum not found. What.", LogLevel.Warn);
            return;
        }

        ModEntry.ModMonitor.Log($"Populating unseen artifacts.");
        _missingArtifacts = Game1.objectData.Keys.Where(static item => LibraryMuseum.IsItemSuitableForDonation(item, true))
            .Select(static item => ItemRegistry.ManuallyQualifyItemId(item, ItemRegistry.type_object)).ToList();

    }
}
