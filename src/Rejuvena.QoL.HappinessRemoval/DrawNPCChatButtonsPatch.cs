using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TeaFramework.Features.Patching;
using TeaFramework.Utilities;
using TeaFramework.Utilities.Extensions;
using Terraria;
using Terraria.ModLoader;

namespace HappinessRemoval
{
    public class DrawNPCChatButtonsPatch : Patch<ILContext.Manipulator>
    {
        public override MethodInfo ModifiedMethod { get; } = typeof(Main).GetCachedMethod("DrawNPCChatButtons");

        protected override ILContext.Manipulator PatchMethod =>
            il =>
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.NPCCheckHappiness"))) {
                    this.LogOpCodeJumpFailure("Terraria.Main", "DrawNPCChatButtons", "ldstr", "\"UI.NPCCheckHappiness\"");
                    return;
                }

                //c.Emit(OpCodes.Pop);
                //c.Emit(OpCodes.Ldstr, string.Empty);
                c.EmitDelegate((string text) => ModContent.GetInstance<HappinessConfig>().ToggleHappiness ? text : "");
            };
    }
}