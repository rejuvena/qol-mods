using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Rejuvena.Gimmick.RecipeShuffler.Cache;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler
{
    [UsedImplicitly]
    public sealed class RecipeShufflerMod : Mod
    {
        public class VanillaRecipeCacheInitializer : ModSystem
        {
            private RecipeShufflerMod Mod => (RecipeShufflerMod) base.Mod;

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

        public RecipeShufflerMod() {
            PacketHandler = new PacketHandler(this);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => PacketHandler.HandlePacket(reader, whoAmI);
    }
}