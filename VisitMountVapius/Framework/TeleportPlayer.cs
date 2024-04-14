using Microsoft.Xna.Framework;

using StardewValley;

namespace VisitMountVapius.Framework;
internal static class TeleportPlayer
{
    /// <summary>
    /// Moves the player to a different location.
    /// </summary>
    /// <param name="loc">Game location this was called from.</param>
    /// <param name="parameters">Parameters.</param>
    /// <param name="who">The farmer in question.</param>
    /// <param name="point">tile location.</param>
    /// <returns>True if handled, false otherwise.</returns>
    internal static bool ApplyCommand(GameLocation loc, string[] parameters, Farmer who, Vector2 point)
    {
        if (!who.IsLocalPlayer)
        {
            return false;
        }

        if (parameters.Length < 4)
        {
            loc.LogTileActionError(parameters, (int)point.X, (int)point.Y, "incorrect number of parameters (expected at least 4)");
            return false;
        }

        // <string area> <int x> <int y> [int facing] [string prerequisite]
        if (!ArgUtility.TryGet(parameters, 1, out string? map, out string? error, false))
        {
            loc.LogTileActionError(parameters, (int)point.X, (int)point.Y, error);
            return false;
        }
        if (Game1.getLocationFromName(map) is not GameLocation destination)
        {
            loc.LogTileActionError(parameters, (int)point.X, (int)point.Y, $"find destination for map {map}");
            return false;
        }

        if (!ArgUtility.TryGetInt(parameters, 2, out int x, out error) || !ArgUtility.TryGetInt(parameters, 3, out int y, out error))
        {
            loc.LogTileActionError(parameters, (int)point.X, (int)point.Y, error);
            return false;
        }

        int? direction = null;
        if (parameters.Length > 4)
        {
            int conditionsStart = 4;
            if (int.TryParse(parameters[4], out int val))
            {
                direction = val;
                conditionsStart = 5;
            }

            if (ArgUtility.TryGetOptionalRemainder(parameters, conditionsStart, out string? condition)
                && !string.IsNullOrWhiteSpace(condition))
            {
                ModEntry.ModMonitor.VerboseLog($"[Teleport] - checking {condition}");
                if (!GameStateQuery.CheckConditions(condition))
                {
                    ModEntry.ModMonitor.VerboseLog($"[Teleport] action failed condition.");
                    return false;
                }
            }
        }

        Game1.warpFarmer(destination.Name, x, y, direction ?? who.FacingDirection, false);
        return true;
    }
}