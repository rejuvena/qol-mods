using Rejuvena.Gimmick.RecipeShuffler.Cache;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler.Compatibility;

public interface ICompatibilityModule : ILoadable
{
    void PreRecipeShuffle(int seed);

    void RecipeShuffle(RecipeCache cache);

    void PostRecipeShuffle(RecipeCache cache);
}