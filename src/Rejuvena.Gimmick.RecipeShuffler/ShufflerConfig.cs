using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Rejuvena.Gimmick.RecipeShuffler
{
    [Label("$Mods.RecipeShuffler.Config.ConfigName")]
    public class ShufflerConfig : ModConfig
    {
        public static ShufflerConfig Instance => ModContent.GetInstance<ShufflerConfig>();

        public override ConfigScope Mode => ConfigScope.ServerSide;

        [UsedImplicitly]
        [Header("$Mods.RecipeShuffler.Config.RestrictionsHeader")]
        [Label("$Mods.RecipeShuffler.Config.BlacklistedItemsLabel")]
        [Tooltip("$Mods.RecipeShuffler.Config.BlacklistedItemsTooltip")]
        public List<ItemDefinition> BlacklistedItems { get; set; } = new();
    }
}