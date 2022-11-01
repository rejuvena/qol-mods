using HappinessRemoval;
using JetBrains.Annotations;
using MonoMod.Cil;
using Terraria.ModLoader;

namespace Rejuvena.QoL.HappinessRemoval;

[UsedImplicitly]
public sealed class HappinessRemovalMod : Mod
{
    public override void Load() {
        base.Load();

        IL.Terraria.Chest.SetupShop += il =>
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.Before, x => x.MatchStloc(0))) {
                this.AddWarning("Terraria.Chest::SetupShop stloc.0");
                return;
            }

            c.EmitDelegate((bool flag) => flag || ModContent.GetInstance<HappinessConfig>().OverridePylon);
            // c.Emit(OpCodes.Stloc_0); // flag
        };

        IL.Terraria.Main.DrawNPCChatButtons += il =>
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.NPCCheckHappiness"))) {
                this.AddWarning("Terraria.Main::DrawNPCChatButtons ldstr \"UI.NPCCheckHappiness\"");
                return;
            }

            //c.Emit(OpCodes.Pop);
            //c.Emit(OpCodes.Ldstr, string.Empty);
            c.EmitDelegate((string text) => ModContent.GetInstance<HappinessConfig>().ToggleHappiness ? text : "");
        };
    }
}