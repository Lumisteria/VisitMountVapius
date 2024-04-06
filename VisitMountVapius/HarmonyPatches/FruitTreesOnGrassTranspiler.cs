using System.Reflection.Emit;

using HarmonyLib;

using StardewModdingAPI;

using StardewValley;

using VisitMountVapius.Framework;

namespace VisitMountVapius.HarmonyPatches;

[HarmonyPatch(typeof(SObject))]
internal static class FruitTreesOnGrassTranspiler
{
    [HarmonyPatch(nameof(SObject.placementAction))]
    private static IEnumerable<CodeInstruction>? Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
    {
        CodeMatcher matcher = new(instructions, gen);

        try
        {
            matcher
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SObject), nameof(SObject.isSapling)))
                )
            .ThrowIfInvalid("isSapling not found")
            .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlantTreesHere))))
            .ThrowIfInvalid("location.CanPlantTrees here not found")
            .MatchStartBackwards(new CodeMatch(OpCodes.Ldstr, "Stone"))
            .ThrowIfInvalid("Tile property checks")
            .Advance(-1);

            // we should be currently in the block text == "Stone"
            // opcodes go
            // ldloc.s text
            // ldstr "Stone"
            // call bool system.string.op_equality,
            // brfalse

            // We want to make this `text == "Stone" || (text == "Grass" && hasOurMapProperty)

            CodeInstruction local = matcher.Instruction.Clone();
            Label jumpPoint = gen.DefineLabel();

            matcher.Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_1), // gameLocation
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FruitTreesOnGrassTranspiler), nameof(IsGrassAndOurMapPropertyExists))),
                new CodeInstruction(OpCodes.Brtrue_S, jumpPoint),
                local
                );

            matcher.MatchStartForward(new CodeMatch(static (instr) => instr.opcode == OpCodes.Brfalse || instr.opcode == OpCodes.Brfalse_S))
            .AddLabelsAt(matcher.Pos + 1, new[] { jumpPoint });
            return matcher.InstructionEnumeration();

        }
        catch (Exception ex)
        {
            ModEntry.ModMonitor.LogError("transpiling SObject.placementAction", ex);
        }
        return null;
    }

    private static bool IsGrassAndOurMapPropertyExists(string property, GameLocation location)
        => property == "Grass" && location.HasMapPropertyWithValue("VMV.FruitTreesOnGrass");
}
