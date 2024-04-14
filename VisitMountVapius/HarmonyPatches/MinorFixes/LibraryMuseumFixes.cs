using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewValley.Locations;

namespace VisitMountVapius.HarmonyPatches.MinorFixes;

[HarmonyPatch(typeof(LibraryMuseum))]
internal static class LibraryMuseumFixes
{

    [HarmonyPatch(nameof(LibraryMuseum.getMuseumDonationBounds))]
    private static void Postfix(ref Rectangle __result)
    {
        if (__result.X > 26)
        {
            __result.X = 26;
        }

        if (__result.Y > 5)
        {
            __result.Y = 5;
        }

        if (__result.Bottom <= 35)
        {
            __result.Height = 35 - __result.Y;
        }

        if (__result.Right <= 50)
        {
            __result.Width = 50 - __result.X;
        }

    }
}
