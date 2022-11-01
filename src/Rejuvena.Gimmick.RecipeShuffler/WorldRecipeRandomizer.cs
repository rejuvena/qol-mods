using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Rejuvena.Gimmick.RecipeShuffler.Cache;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler
{
    [UsedImplicitly]
    public class WorldRecipeRandomizer : ModPlayer
    {
        protected RecipeShufflerMod ShufflerMod => (RecipeShufflerMod) Mod;

        protected Func<bool>? Task = null;

        public override void OnEnterWorld(Player player) {
            if (!AppliesToUs(player)) return;

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                WorldGen.currentWorldSeed = "";

                // Send request packet to the server.
                ModPacket packet = ShufflerMod.GetPacket();
                packet.Write((byte) PacketHandler.PacketType.RequestSeedFromServer);
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

        public override void PreUpdate() {
            if (AppliesToUs(Player) && Task != null && Task()) Task = null;
        }

        protected void ShuffleRecipes() {
            void Log(string msg) {
                Main.NewText($"[{Mod.Name}] {msg}");
                ShufflerMod.Logger.Debug($"[{Mod.Name}] {msg}");
            }

            if (Debugger.IsAttached) Log("[Debug] " + Language.GetTextValue("Mods.RecipeShuffler.Chat.FoundWorldSeed", WorldGen.currentWorldSeed));

            if (!int.TryParse(WorldGen.currentWorldSeed, out int seed)) seed = Crc32.Calculate(WorldGen.currentWorldSeed);

            Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.LoadingCache", seed));

            if (ShufflerMod.Caches.ContainsKey(seed)) {
                if (ShufflerMod.Caches[seed].VerifyIntegrity(ShufflerMod.VanillaCache)) {
                    Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.FailedVerification"));
                    goto IntegFail;
                }

                Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.UsingVerified"));

                return;
            }

        IntegFail:
            RecipeCache cache = new();
            cache.SetRecipes(ShufflerMod.VanillaCache.ReadonlyRecipes);
            cache.ShuffleRecipes(seed);

            if (!cache.VerifyIntegrity(ShufflerMod.VanillaCache)) {
                Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.FailedIntegrity"));
                return;
            }

            Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.SuccessfullyShuffled"));

            ShufflerMod.VanillaCache.SetRecipes(cache.ReadonlyRecipes);
            ShufflerMod.Caches[seed] = cache;

            Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.AppliedNew"));
        }
        
        private static bool AppliesToUs(Entity player) {
            return Main.netMode == NetmodeID.SinglePlayer || (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer);
        }
    }
}