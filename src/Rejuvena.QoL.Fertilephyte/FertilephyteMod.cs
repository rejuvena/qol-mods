using System.Collections.Generic;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.QoL.Fertilephyte;

[UsedImplicitly]
public class FertilephyteMod : Mod
{
    private class ErrorReporter : ModPlayer
    {
        private new FertilephyteMod Mod => (FertilephyteMod) base.Mod;

        public override bool IsLoadingEnabled(Mod mod) {
            if (mod is FertilephyteMod) return true;

            mod.Logger.Warn($"Attempted to load type '{nameof(ErrorReporter)}' from mod '{mod.Name}'.");
            return false;
        }

        public override void OnEnterWorld(Player player) {
            static void Log(string text) => Main.NewText(text, Colors.RarityRed);

            base.OnEnterWorld(player);

            if (Mod.Warnings.Count == 0) return;

            Log(Language.GetTextValue("Mods.Fertilephyte.Errors.ErrorWarnMessage"));
            Mod.Warnings.ForEach(Log);
        }
    }

    private readonly List<string> Warnings = new();

    public override void Load() {
        base.Load();

        IL.Terraria.WorldGen.hardUpdateWorld += il =>
        {
            /*
             * Objectives:
             *  - [x] Make Chlorophyte spawn with a 1/2 chance instead of a 1/3 chance if Plantera has been defeated.
             */
            ILCursor c = new(il);
            
            // Jump to our reliable anchor point (ldc.i4 211) x2.
            // TODO: It's definitely possible to make this safer by matching ldloc.x followed directly by ldc.i4 211. Do this maybe?
            if (!c.TryGotoNext(x => x.MatchLdcI4(TileID.Chlorophyte))) {
                LogILError("Terraria.WorldGen::hardUpdateWorld ldc.i4 211 (1)");
                return;
            }
            
            if (!c.TryGotoNext(x => x.MatchLdcI4(TileID.Chlorophyte))) {
                LogILError("Terraria.WorldGen::hardUpdateWorld ldc.i4 211 (2)");
                return;
            }
            
            // Jump to ldc.i4 emission and push 2 if Plantera is killed, otherwise push 3 (the original value).
            // TODO: Return (x - 1) instead of 2 in case the value changes?
            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(out _))) {
                LogILError("After: Terraria.WorldGen::hardUpdateWorld ldc.i4 _");
                return;
            }

            c.EmitDelegate((int x) => NPC.downedPlantBoss ? 2 : x);
        };

        IL.Terraria.WorldGen.Chlorophyte += il =>
        {
            /* Objectives:
             *  - [x] Remove the check for cavern layer height if Plantera has been defeated.
             *  - [x] Increase near radius limit to 60 (mult. 1.5).
             *  - [x] Increase far radius limit to 195 (mult 1.5).
             *  - [x] For completion's sake, keep radii the same (for now).
             */
            ILCursor c = new(il);

            // Remove the check for cavern layer height if Plantera has been defeated.
            //  - [x] Skip right before nearest branch.
            //  - [x] Consume float value pushed right before it.
            //  - [x] Replace the regular height check with float.MinValue to ensure the position is always greater.
            ILLabel? branch = null;
            if (!c.TryGotoNext(MoveType.Before, x => x.MatchBgeUn(out branch))) {
                LogILError("Before: Terraria.WorldGen::Chlorophyte bge.un.s");
                return;
            }

            if (branch is null) {
                LogILError("Terraria.WorldGen::Chlorophyte Failed to match branch!");
                return;
            }

            c.EmitDelegate((float orig) => NPC.downedPlantBoss ? float.MinValue : orig);

            // Match ldc.i4 opcode in the branch block to determine radius limit variables.
            if (!c.TryGotoNext(x => x.MatchLdcI4(out _))) {
                LogILError("Terraria.WorldGen::Chlorophyte ldc.i4 _ (1)");
                return;
            }

            int limit1Index = -1;
            if (!c.TryGotoPrev(x => x.MatchLdloc(out limit1Index))) {
                LogILError("Previous: Terraria.WorldGen::Chlorophyte ldloc _ (1)");
                return;
            }

            // Do it once more...
            if (!c.TryGotoNext(x => x.MatchLdcI4(out _))) {
                LogILError("Terraria.WorldGen::Chlorophyte ldc.i4 _ (2)");
                return;
            }

            int limit2Index = -1;
            if (!c.TryGotoPrev(x => x.MatchLdloc(out limit2Index))) {
                LogILError("Previous: Terraria.WorldGen::Chlorophyte ldloc _ (2)");
                return;
            }

            if (limit1Index == -1) {
                LogILError("Terraria.WorldGen::Chlorophyte Failed to match index of limit 1!");
                return;
            }

            if (limit2Index == -1) {
                LogILError("Terraria.WorldGen::Chlorophyte Failed to match index of limit 2!");
                return;
            }

            if (!c.TryGotoNext(MoveType.Before, x => x.MatchLdcI4(0))) {
                LogILError("Before: Terraria.WorldGen::Chlorophyte ldc.i4 0");
                return;
            }

            if (c.Next != branch.Target) {
                LogILError($"Terraria.WorldGen::Chlorohpyte Unexpected cursor position! branch (expected): {branch.Target}, cursor (result): {c.Next}");
                return;
            }
            
            // Multiply each limit value by 1.5 if Plantera has been killed.
            void EmitAndMultiplyValue(int local) {
                c.Emit(OpCodes.Ldloc, local); // push local
                c.Emit(OpCodes.Conv_R8); // convert to float
                // c.Emit(OpCodes.Ldc_R8, 1.5); // push value to multiply by
                c.EmitDelegate(() => NPC.downedPlantBoss ? 1.5 : 1.0); // push 1.5 if Plantera has been killed, otherwise 1.0 (value to multiply by)
                c.Emit(OpCodes.Mul); // multiply
                c.Emit(OpCodes.Conv_I4); // convert back to int
                c.Emit(OpCodes.Stloc, local); // set local
            }
            
            EmitAndMultiplyValue(limit1Index);
            EmitAndMultiplyValue(limit2Index);
        };
    }

    private void LogILError(string text) {
        Warnings.Add(text);
        Logger.Warn(text);
    }

    // Reverse-engineered Terraria.WorldGen::Chlorophyte
    // Return value determines if a Chlorophyte tile maybe placed based on analyzing the radius.
    /*public static bool Chlorophyte(int centerX, int centerY) {
        int limitNear = 40;
        int radiusNear = 35;
        
        int limitFar = 85;
        int radiusFar = 130;
        
        int foundTiles = 0;

        // If above cavern layer, decrease the limit by half and raise the radius by half.
        if (centerY < Main.rockLayer) {
            limitNear /= 2;
            radiusNear = (int) (radiusNear * 1.5);
            
            limitFar = (int) (limitFar * 1.5);
            radiusFar /= 2;
        }
        
        // Loop through near radius and find all Chlorophyte tiles.
        for (int x = centerX - radiusNear; x < centerX + radiusNear; x++)
        for (int y = centerY - radiusNear; y < centerY + radiusNear; y++)
            if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Chlorophyte)
                foundTiles++;

        // If found tiles is greater than the near limit, return false and skip far radius check.
        if (foundTiles > limitNear) return false;
        
        foundTiles = 0;
        
        // Loop through far radius and find all Chlorophyte tiles.
        for (int x = centerX - limitFar; x < centerX + limitFar; x++)
        for (int y = centerY - limitFar; y < centerY + limitFar; y++)
            if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Chlorophyte)
                foundTiles++;

        // If found tiles is greater than the far limit, return false, otherwise return true.
        return foundTiles <= radiusFar;
    }*/
}