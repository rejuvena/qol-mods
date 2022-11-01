using System;
using JetBrains.Annotations;
using Rejuvena.Gimmick.RecipeShuffler.Cache;
using Rejuvena.Gimmick.RecipeShuffler.Compatibility.MagicStorage.Providers;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler.Compatibility.MagicStorage;

[UsedImplicitly]
[JITWhenModsEnabled("MagicStorage")]
public sealed class MagicStorageModule : ICompatibilityModule
{
    private Mod? MagicStorageMod;
    private IMagicStorageCompatibilityProvider? CompatibilityProvider;

    bool ILoadable.IsLoadingEnabled(Mod mod) {
        return ModLoader.TryGetMod("MagicStorage", out MagicStorageMod);
    }

    private IMagicStorageCompatibilityProvider MakeCompatibilityProvider() {
        if (MagicStorageMod is null) return new NoActionCompatibilityProvider();
        if (MagicStorageMod.Version >= new Version(0, 5, 7, 10)) return new ApiCallCompatibilityProvider(MagicStorageMod);
        return new ManualCallCompatibilityProvider(MagicStorageMod);
    }
    
    #region ICompatibilityModule Impl

    void ICompatibilityModule.PreRecipeShuffle(int seed) {
    }

    void ICompatibilityModule.RecipeShuffle(RecipeCache cache) {
    }

    void ICompatibilityModule.PostRecipeShuffle(RecipeCache cache) {
        CompatibilityProvider ??= MakeCompatibilityProvider();
        CompatibilityProvider.ReCacheMagicStorageRecipes();
    }

    #endregion
    
    #region ILoadable Impl

    void ILoadable.Load(Mod mod) {
    }

    void ILoadable.Unload() {
    }

    #endregion
}