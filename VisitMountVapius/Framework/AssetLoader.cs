using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using VisitMountVapius.Models;

namespace VisitMountVapius.Framework;

internal static class AssetLoader
{
    internal static IAssetName LocationExtensions { get; private set; } = null!;

    internal static IAssetName CropExtensions { get; private set; } = null!;

    internal static void Init(IGameContentHelper parser)
    {
        LocationExtensions = parser.ParseAssetName("Mods/VisitMountVapius/LocationExtensions");
        CropExtensions = parser.ParseAssetName("Mods/VisitMountVapius/CropExtensions");
    }

    internal static void Load(AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(LocationExtensions))
        {
            e.LoadFrom(static () => new Dictionary<string, LocationDataExtensions>(), AssetLoadPriority.Exclusive);
        }
        else if (e.NameWithoutLocale.IsEquivalentTo(CropExtensions))
        {
            e.LoadFrom(static () => new Dictionary<string, CropDataExtensions>(), AssetLoadPriority.Exclusive);
        }
    }

    internal static Dictionary<string, LocationDataExtensions> GetLocationData() => Game1.temporaryContent.Load<Dictionary<string, LocationDataExtensions>>(LocationExtensions.BaseName);

    internal static Dictionary<string, CropDataExtensions> GetCropData() => Game1.temporaryContent.Load<Dictionary<string,  CropDataExtensions>>(CropExtensions.BaseName);
}