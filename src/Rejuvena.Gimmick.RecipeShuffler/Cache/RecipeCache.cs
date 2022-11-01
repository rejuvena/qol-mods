using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler.Cache
{
    /// <summary>
    ///     Object representing a shuffled recipe state.
    /// </summary>
    public class RecipeCache
    {
        /// <summary>
        ///     A collection of cached recipes.
        /// </summary>
        protected readonly List<Recipe> Recipes = new();

        /// <summary>
        ///     A readonly collection of cached recipes.
        /// </summary>
        public ReadOnlyCollection<Recipe> ReadonlyRecipes => Recipes.AsReadOnly();

        /// <summary>
        ///     Verifies the integrity of a recipe cache against another recipe cache.
        /// </summary>
        public virtual bool VerifyIntegrity(RecipeCache cache) => Recipes.Count == cache.ReadonlyRecipes.Count;

        /// <summary>
        ///     Shuffles the <see cref="Recipes"/> collection, given the seed.
        /// </summary>
        public virtual void ShuffleRecipes(int seed) {
            static bool Allow(Recipe r) => ShufflerConfig.Instance.BlacklistedItems.All(item => item.Type != r.createItem.type);

            Random rand = new(seed);

            Recipe[] whitelist = Recipes.Where(Allow).ToArray();
            Item[] results = whitelist
                             // .Where(x => x.createItem is not null && x.createItem.type != ItemID.None)
                            .Select(x => x.createItem)
                            .OrderBy(x => rand.Next())
                            .ToArray();

            FieldInfo setupRecipes = typeof(RecipeLoader).GetField(nameof(RecipeLoader.setupRecipes), BindingFlags.NonPublic | BindingFlags.Static)!;

            // RecipeLoader.setupRecipes = true;
            setupRecipes.SetValue(null, true);

            for (int i = 0; i < whitelist.Length; i++) {
                Recipe recipe = (Recipe) whitelist[i].MemberwiseClone();
                recipe.createItem = results[i];
                whitelist[i] = recipe;
            }

            List<Recipe> shuffled = whitelist.ToList();
            shuffled.AddRange(Recipes.Where(x => !Allow(x)));
            SetRecipes(shuffled);

            // RecipeLoader.setupRecipes = false;
            setupRecipes.SetValue(null, false);
        }

        /// <summary>
        ///     Updates a <see cref="Recipes"/> collection externally.
        /// </summary>
        public virtual void SetRecipes(IEnumerable<Recipe> recipes) {
            Recipes.Clear();
            Recipes.AddRange(recipes.Where(x => x.createItem != null && x.createItem.type != ItemID.None));
        }
    }
}