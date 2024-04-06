namespace VisitMountVapius.Interfaces;

using StardewModdingAPI;

/// <summary>
/// The API for this mod.
/// </summary>
public interface IEventTesterAPI
{
    /// <summary>
    /// Registers an asset to be analyzed by the GSQ checker.
    /// </summary>
    /// <param name="assetName">The asset to analyze.</param>
    /// <param name="filter">A filter to select which string fields/properties should be considered GSQ fields.</param>
    /// <returns>true if added, false otherwise.</returns>
    public bool RegisterAsset(IAssetName assetName, Func<string, bool>? filter = null);

    /// <summary>
    /// Registers an asset to be analyzed by the GSQ checker.
    /// </summary>
    /// <param name="assetName">The asset to analyze.</param>
    /// <param name="additionalGSQNames">Additional strings that may correspond to fields that should be consider GSQ fields.</param>
    /// <returns>True if added, false otherwise.</returns>
    public bool RegisterAsset(IAssetName assetName, HashSet<string> additionalGSQNames);

    /// <summary>
    /// Removes an asset from analysis by the GSQ checker.
    /// </summary>
    /// <param name="assetName">Asset to remove.</param>
    /// <returns>True if removed, false otherwise.</returns>
    public bool RemoveAsset(IAssetName assetName);
}