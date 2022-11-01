using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Rejuvena.Gimmick.RecipeShuffler.Cache;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler;

[UsedImplicitly]
public sealed class WorldRecipeRandomizer : ModPlayer
{
    private new RecipeShufflerMod Mod => (RecipeShufflerMod) base.Mod;

    private Func<bool>? Task;

    public override bool IsLoadingEnabled(Mod mod) {
        if (mod is RecipeShufflerMod) return true;

        mod.Logger.Warn($"Attempted to load type '{nameof(WorldRecipeRandomizer)}' from mod '{mod.Name}'.");
        return false;
    }

    public override void OnEnterWorld(Player player) {
        if (!AppliesToUs(player)) return;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            WorldGen.currentWorldSeed = "";

            // Send request packet to the server.
            ModPacket packet = Mod.GetPacket();
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

    private void ShuffleRecipes() {
        void Log(string msg) {
            Main.NewText($"[{((ModType) this).Mod.Name}] {msg}");
            Mod.Logger.Debug($"[{((ModType) this).Mod.Name}] {msg}");
        }

        if (Debugger.IsAttached) Log("[Debug] " + Language.GetTextValue("Mods.RecipeShuffler.Chat.FoundWorldSeed", WorldGen.currentWorldSeed));

        if (!int.TryParse(WorldGen.currentWorldSeed, out int seed)) seed = Crc32.Calculate(WorldGen.currentWorldSeed);

        Mod.OnPreRecipeShuffle(seed);

        Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.LoadingCache", seed));

        if (Mod.Caches.ContainsKey(seed)) {
            if (Mod.Caches[seed].VerifyIntegrity(Mod.VanillaCache)) {
                Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.FailedVerification"));
                goto IntegFail;
            }

            // eh... good enough
            Mod.OnRecipeShuffle(Mod.Caches[seed]);
            Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.UsingVerified"));
            Mod.OnPostRecipeShuffle(Mod.Caches[seed]);
            return;
        }

    IntegFail:
        RecipeCache cache = new();
        cache.SetRecipes(Mod.VanillaCache.ReadonlyRecipes);
        cache.ShuffleRecipes(seed);
        Mod.OnRecipeShuffle(cache);

        if (!cache.VerifyIntegrity(Mod.VanillaCache)) {
            Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.FailedIntegrity"));
            return;
        }

        Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.SuccessfullyShuffled"));

        Mod.VanillaCache.SetRecipes(cache.ReadonlyRecipes);
        Mod.Caches[seed] = cache;
        Mod.OnPostRecipeShuffle(cache);

        Log(Language.GetTextValue("Mods.RecipeShuffler.Chat.AppliedNew"));
    }

    private static bool AppliesToUs(Entity player) {
        return Main.netMode == NetmodeID.SinglePlayer || (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer);
    }
}