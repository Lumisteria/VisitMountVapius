using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using VisitMountVapius.Models;

namespace VisitMountVapius.Framework;

internal static class AssetLoader
{
    private static IAssetName locationExtensions = null!;

    internal static void Init(IGameContentHelper parser)
        => locationExtensions = parser.ParseAssetName("Mods/VisitMountVapius/LocationExtensions");

    internal static void Load(AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(locationExtensions))
        {
            e.LoadFrom(static () => new Dictionary<string, LocationDataExtensions>(), AssetLoadPriority.Exclusive);
        }
    }

    internal static Dictionary<string, LocationDataExtensions> GetLocationData() => Game1.temporaryContent.Load<Dictionary<string, LocationDataExtensions>>(locationExtensions.BaseName);
}