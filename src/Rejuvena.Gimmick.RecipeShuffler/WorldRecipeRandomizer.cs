using RecipeShuffler.Cache;
using ReLogic.Utilities;
using System;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace RecipeShuffler
{
    public class WorldRecipeRandomizer : ModPlayer
    {
        protected RecipeShuffler Shuffler => (RecipeShuffler)Mod;

        protected Func<bool>? Task = null;

        public override void OnEnterWorld(Player player)
        {
            if (!AppliesToUs(player)) return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                WorldGen.currentWorldSeed = "";

                // Send request packet to the server.
                ModPacket packet = Shuffler.GetPacket();
                packet.Write((byte)PacketHandler.PacketType.RequestSeedFromServer);
                packet.Send();

                Task = () =>
                {
                    if (string.IsNullOrEmpty(WorldGen.currentWorldSeed)) return false;

                    ShuffleRecipes();
                    return true;
                };
            }
            else if (Main.netMode == NetmodeID.SinglePlayer) ShuffleRecipes();
        }

        public override void PreUpdate()
        {
            if (AppliesToUs(Player) && Task != null && Task()) Task = null;
        }

        protected bool AppliesToUs(Player player)
        {
            return Main.netMode == NetmodeID.SinglePlayer || (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer);
        }

        protected void ShuffleRecipes()
        {
            void Log(string msg)
            {
                Main.NewText($"[{Mod.Name}] {msg}");
                Shuffler.Logger.Debug($"[{Mod.Name}] {msg}");
            }

            if (Debugger.IsAttached) Log("[Debug] Found world seed: " + WorldGen.currentWorldSeed);

            if (!int.TryParse(WorldGen.currentWorldSeed, out int seed)) seed = Crc32.Calculate(WorldGen.currentWorldSeed);

            Log($"Loading recipe cache for world seed: {seed}");

            if (Shuffler.Caches.ContainsKey(seed))
            {
                if (Shuffler.Caches[seed].VerifyIntegrity(Shuffler.VanillaCache))
                {
                    Log("Failed to verify pre-existing recipe cache for world, going to re-shuffle.");
                    goto IntegFail;
                }

                Log("Using verified pre-existing recipe cache for world.");

                return;
            }

        IntegFail:
            RecipeCache cache = new();
            cache.SetRecipes(Shuffler.VanillaCache.ReadonlyRecipes);
            cache.ShuffleRecipes(seed);

            if (!cache.VerifyIntegrity(Shuffler.VanillaCache))
            {
                Log("Couldn't verify integrity of brand-new recipe cache, panicking!");
                return;
            }

            Log("Successfully shuffled and verified newly-created recipe cache.");

            Shuffler.VanillaCache.SetRecipes(cache.ReadonlyRecipes);
            Shuffler.Caches[seed] = cache;

            Log("Applied new recipe cache to the current world.");
        }
    }
}