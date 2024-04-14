using HarmonyLib;

using StardewValley;
using StardewValley.Extensions;

using VisitMountVapius.Framework;
using VisitMountVapius.Models;

namespace VisitMountVapius.HarmonyPatches;

[HarmonyPatch(typeof(Crop))]
internal static class CropPatches
{
    [HarmonyPatch(nameof(Crop.harvest))]
    private static void Postfix(Crop __instance, ref bool __result)
    {
        if (__instance.forageCrop.Value)
        {
            return;
        }

        if (__instance.Dirt is null || __instance.Dirt.HasFertilizer())
        {
            return;
        }

        if (!__result && (__instance.currentPhase.Value >= __instance.phaseDays.Count - 1 && (!__instance.fullyGrown.Value || __instance.dayOfCurrentPhase.Value <= 0)))
        {
            return;
        }

        string cropID = __instance.isWildSeedCrop() ? __instance.whichForageCrop.Value : __instance.netSeedIndex.Value;
        CropDataExtensions? data = AssetLoader.GetCropData().GetValueOrDefault(cropID);

        if (data?.FertilizerData is not { } possibleFertilizers)
        {
            return;
        }

        ModEntry.ModMonitor.VerboseLog($"Checking fertilizers for {cropID}");

        foreach ((string fertilizer, double chance) in possibleFertilizers)
        {
            if (Random.Shared.NextBool(chance) && __instance.Dirt.CanApplyFertilizer(fertilizer))
            {
                __instance.Dirt.plant(fertilizer, Game1.player, true);
                return;
            }
        }
    }
}
