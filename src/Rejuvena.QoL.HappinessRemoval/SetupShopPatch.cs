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
    public class SetupShopPatch : Patch<ILContext.Manipulator>
    {
        public override MethodInfo ModifiedMethod { get; } = typeof(Chest).GetCachedMethod("SetupShop");

        protected override ILContext.Manipulator PatchMethod =>
            il =>
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(x => x.MatchStloc(0))) {
                    this.LogOpCodeJumpFailure("Terraria.Chest", "SetupShop", "stloc", "0");
                    return;
                }

                c.EmitDelegate((bool flag) => flag || ModContent.GetInstance<HappinessConfig>().OverridePylon);
                // c.Emit(OpCodes.Stloc_0); // flag
            };
    }
}