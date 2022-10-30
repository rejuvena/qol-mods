using System.Collections.Generic;
using System.IO;
using RecipeShuffler.Cache;
using Terraria.ModLoader;

namespace RecipeShuffler
{
    public class RecipeShuffler : Mod
    {
        /// <summary>
        ///		Represents the Recipe cache in its vanilla state. Should be referenced as the "natural" state of recipes.
        /// </summary>
        /// <remarks>
        ///		Useful for re-shuffling recipes between worlds.
        /// </remarks>
        public RecipeCache VanillaCache { get; internal set; } = null!; // Never null when used.

        public PacketHandler PacketHandler;

        public readonly Dictionary<int, RecipeCache> Caches = new();

        public RecipeShuffler()
        {
            PacketHandler = new(this);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => PacketHandler.HandlePacket(reader, whoAmI);
    }

    public class YetAnotherTMLChangeIDidNotAskFor : ModSystem
    {
        public override void PostAddRecipes() {
            base.PostAddRecipes();
            
            if (Mod is RecipeShuffler shuffler) shuffler.VanillaCache = new VanillaRecipeCache();
        }
    }
}