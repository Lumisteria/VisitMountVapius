using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

using VisitMountVapius.Framework;

namespace VisitMountVapius.HarmonyPatches;

/// <summary>
/// Patches against fruit trees.
/// </summary>
[HarmonyPatch(typeof(FruitTree))]
internal static class FruitTreePatches
{
    private static readonly PerScreen<int> Ticks = new(static () => -1);
    private static readonly PerScreen<int> Attempts = new(static () => 0);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FruitTree.shake))]
    private static bool PrefixShake(FruitTree __instance)
    {
        int x = (int)__instance.Tile.X;
        int y = (int)__instance.Tile.Y;
        if (__instance.Location?.doesTileHaveProperty(x, y, "vmv.FruitTreeShake", "Back") is string message)
        {
            if (Game1.ticks > Ticks.Value + 120)
            {
                Attempts.Value = 0;
                Ticks.Value = Game1.ticks;
            }
            else
            {
                Attempts.Value++;
            }

            if (Attempts.Value > 2)
            {
                ShowMessage(message);
                Attempts.Value = 0;
            }

            ShakeTree(__instance);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FruitTree.performToolAction))]
    private static bool PrefixCut(FruitTree __instance, Tool t)
    {
        if (t is MeleeWeapon)
        {
            return true;
        }

        int x = (int)__instance.Tile.X;
        int y = (int)__instance.Tile.Y;
        if (__instance.Location?.doesTileHaveProperty(x, y, "vmv.FruitTreeCut", "Back") is string message)
        {
            ShowMessage(message);
            ShakeTree(__instance);
            return false;
        }

        return true;
    }

    private static void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Game1.showRedMessage(I18n.FruitTree_Forbidden());
        }
        else if (message.TrySplitOnce(':', out ReadOnlySpan<char> first, out ReadOnlySpan<char> second))
        {
            string name = first.Trim().ToString();
            NPC? npc = Game1.getCharacterFromName(name);
            if (npc is null)
            {
                try
                {
                    string? portrait_path = null;
                    if (Game1.characterData.TryGetValue(name, out CharacterData? data))
                    {
                        portrait_path = data.TextureName;
                    }
                    if (string.IsNullOrEmpty(portrait_path))
                    {
                        portrait_path = name;
                    }

                    npc = new NPC(
                    sprite: null,
                    position: Vector2.Zero,
                    defaultMap: string.Empty,
                    facingDirection: 0,
                    name,
                    datable: false,
                    portrait: Game1.temporaryContent.Load<Texture2D>("Portraits\\" + portrait_path));
                }
                catch (Exception ex)
                {
                    ModEntry.ModMonitor.LogError($"creating NPC {name}", ex);
                    return;
                }
            }
            Game1.DrawDialogue(new(npc, null, second.Trim().ToString().ParseTokens()));
        }
        else
        {
            Game1.drawObjectDialogue(message.Trim().ParseTokens());
        }
    }

    private static void ShakeTree(FruitTree tree)
    {
        Farmer player = Game1.player;

        tree.shakeLeft.Value = player.StandingPixel.X > (tree.Tile.X + 0.5f) * 64f || (player.Tile.X == tree.Tile.X && Game1.random.NextBool());
        tree.maxShake = tree.growthStage.Value >= 4 ? MathF.PI / 128.0f : MathF.PI / 64f;
    }
}