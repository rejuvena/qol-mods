using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RecipeShuffler
{
    [Label("Server-Side Suffler Config")]
    public class ShufflerConfig : ModConfig
    {
        public static ShufflerConfig Get => ModContent.GetInstance<ShufflerConfig>();
        
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Suffling Restrictions")]
        [Label("Shuffler Blacklist")]
        [Tooltip("Recipe results to filter out from shuffling.")]
        public List<ItemDefinition> BlacklistedItems { get; set; } = new();
    }
}