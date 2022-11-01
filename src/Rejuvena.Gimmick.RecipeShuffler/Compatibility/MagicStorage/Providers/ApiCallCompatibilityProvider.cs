using System;
using System.Reflection;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler.Compatibility.MagicStorage.Providers;

public class ApiCallCompatibilityProvider : IMagicStorageCompatibilityProvider
{
    private readonly IMagicStorageCompatibilityProvider FallbackProvider;
    private readonly Mod? MagicStorageMod;

    public ApiCallCompatibilityProvider(Mod? magicStorageMod) {
        MagicStorageMod = magicStorageMod;
        FallbackProvider = new ManualCallCompatibilityProvider(magicStorageMod);
    }

    void IMagicStorageCompatibilityProvider.ReCacheMagicStorageRecipes() {
        try {
            if (MagicStorageMod is null) throw new Exception(); // skip to catch block
            Type? magicCacheType = MagicStorageMod.Code.GetType("MagicStorage.Common.Systems.MagicCache");
            MethodInfo? recalculateMethod = magicCacheType?.GetMethod("RecalculateRecipeCaches", BindingFlags.Public | BindingFlags.Static);
            
            if (magicCacheType is null || recalculateMethod is null) throw new Exception(); // skip to catch block
            recalculateMethod.Invoke(null, null);
        }
        catch {
            FallbackProvider.ReCacheMagicStorageRecipes();
        }
    }
}