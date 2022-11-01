using System;
using System.Reflection;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler.Compatibility.MagicStorage.Providers;

public class ManualCallCompatibilityProvider : IMagicStorageCompatibilityProvider
{
    private readonly Mod? MagicStorageMod;

    public ManualCallCompatibilityProvider(Mod? magicStorageMod) {
        MagicStorageMod = magicStorageMod;
    }

    void IMagicStorageCompatibilityProvider.ReCacheMagicStorageRecipes() {
        if (MagicStorageMod is null) return;
        if (!MagicStorageMod.TryFind("MagicCache", out ModSystem magicCacheSystem)) return;
        magicCacheSystem.PostSetupRecipes();

        Type? storageGuiType = MagicStorageMod.Code.GetType("MagicStorage.StorageGUI");
        FieldInfo? needRefreshField = storageGuiType?.GetField("needRefresh", BindingFlags.Public | BindingFlags.Static);
        if (storageGuiType is null || needRefreshField is null) return;
        
        needRefreshField.SetValue(null, true);
    }
}