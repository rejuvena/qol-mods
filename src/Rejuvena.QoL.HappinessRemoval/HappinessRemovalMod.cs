using System.Collections.Generic;
using JetBrains.Annotations;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.QoL.HappinessRemoval;

[UsedImplicitly]
public sealed class HappinessRemovalMod : Mod
{
    private class ErrorReporter : ModPlayer
    {
        private new HappinessRemovalMod Mod => (HappinessRemovalMod) base.Mod;

        public override bool IsLoadingEnabled(Mod mod) {
            if (mod is HappinessRemovalMod) return true;

            mod.Logger.Warn($"Attempted to load type '{nameof(ErrorReporter)}' from mod '{mod.Name}'.");
            return false;
        }

        public override void OnEnterWorld(Player player) {
            static void Log(string text) => Main.NewText(text, Colors.RarityRed);

            base.OnEnterWorld(player);
                
            if (Mod.Warnings.Count == 0) return;

            Log(Language.GetTextValue("Mods.HappinessRemoval.Errors.ErrorWarnMessage"));
            Mod.Warnings.ForEach(Log);
        }
    }

    private readonly List<string> Warnings = new();

    public override void Load() {
        base.Load();

        IL.Terraria.Chest.SetupShop += il =>
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.Before, x => x.MatchStloc(0))) {
                LogILError("Terraria.Chest::SetupShop stloc.0");
                return;
            }

            c.EmitDelegate((bool flag) => flag || ModContent.GetInstance<HappinessConfig>().OverridePylon);
            // c.Emit(OpCodes.Stloc_0); // flag
        };

        IL.Terraria.Main.DrawNPCChatButtons += il =>
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.NPCCheckHappiness"))) {
                LogILError("Terraria.Main::DrawNPCChatButtons ldstr \"UI.NPCCheckHappiness\"");
                return;
            }

            //c.Emit(OpCodes.Pop);
            //c.Emit(OpCodes.Ldstr, string.Empty);
            c.EmitDelegate((string text) => ModContent.GetInstance<HappinessConfig>().ToggleHappiness ? text : "");
        };
    }

    private void LogILError(string text) {
        Warnings.Add(text);
        Logger.Warn(text);
    }
}