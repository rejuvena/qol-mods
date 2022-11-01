using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Rejuvena.Gimmick.RecipeShuffler.Cache;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler;

[UsedImplicitly]
public sealed class RecipeShufflerMod : Mod
{
    [UsedImplicitly]
    public class VanillaRecipeCacheInitializer : ModSystem
    {
        private new RecipeShufflerMod Mod => (RecipeShufflerMod) base.Mod;

        public override bool IsLoadingEnabled(Mod mod) {
            if (mod is RecipeShufflerMod) return true;

            mod.Logger.Warn($"Attempted to load type '{nameof(VanillaRecipeCacheInitializer)}' from mod '{mod.Name}'.");
            return false;
        }

        public override void PostAddRecipes() {
            base.PostAddRecipes();
            Mod.VanillaCache = new VanillaRecipeCache();
        }
    }

    /// <summary>
    ///		Represents the Recipe cache in its vanilla state. Should be referenced as the "natural" state of recipes.
    /// </summary>
    /// <remarks>
    ///		Useful for re-shuffling recipes between worlds.
    /// </remarks>
    public RecipeCache VanillaCache { get; internal set; } = null!; // Never null when used.

    public PacketHandler PacketHandler { get; }

    public Dictionary<int, RecipeCache> Caches { get; } = new();

    public event Action<int>? PreRecipeShuffle;

    public event Action<RecipeCache>? RecipeShuffle;

    public event Action<RecipeCache>? PostRecipeShuffle;

    public RecipeShufflerMod() {
        PacketHandler = new PacketHandler(this);
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) => PacketHandler.HandlePacket(reader, whoAmI);

    public void OnPreRecipeShuffle(int seed) {
        PreRecipeShuffle?.Invoke(seed);
    }

    public void OnRecipeShuffle(RecipeCache cache) {
        RecipeShuffle?.Invoke(cache);
    }

    public void OnPostRecipeShuffle(RecipeCache cache) {
        PostRecipeShuffle?.Invoke(cache);
    }
}