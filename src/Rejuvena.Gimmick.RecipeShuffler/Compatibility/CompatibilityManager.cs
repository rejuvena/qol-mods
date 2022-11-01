using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler.Compatibility;

[UsedImplicitly]
public sealed class CompatibilityManager : ModSystem
{
    private new RecipeShufflerMod Mod => (RecipeShufflerMod) base.Mod;

    public override bool IsLoadingEnabled(Mod mod) {
        if (mod is RecipeShufflerMod) return true;

        mod.Logger.Warn($"Attempted to load type '{nameof(CompatibilityManager)}' from mod '{mod.Name}'.");
        return false;
    }

    public override void OnModLoad() {
        base.OnModLoad();

        IEnumerable<ICompatibilityModule>? modules = Mod.GetContent<ICompatibilityModule>();

        if (modules is null) return;
        foreach (ICompatibilityModule module in modules) {
            Mod.PreRecipeShuffle += module.PreRecipeShuffle;
            Mod.RecipeShuffle += module.RecipeShuffle;
            Mod.PostRecipeShuffle += module.PostRecipeShuffle;
        }
    }
}