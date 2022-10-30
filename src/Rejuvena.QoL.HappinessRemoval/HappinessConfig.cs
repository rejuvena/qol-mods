using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace Rejuvena.QoL.HappinessRemoval
{
    [Label("Happiness Config")]
    public class HappinessConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Happiness")]
        [Label("Use Vanilla Happiness")]
        [Tooltip("If enabled, Vanilla happiness values will be used instead of being overridden by this mod.")]
        [DefaultValue(false)]
        public bool ToggleHappiness = false;

        [Label("NPC Happiness Level")]
        [Tooltip("The lower this slider is, the happier NPCs will be.")]
        [Slider]
        [DefaultValue(0.75f)]
        [Range(0.5f, 2f)]
        public float NpcHappiness = 0.75f;

        [Label("Override Pylon Happiness")]
        [Tooltip("Forces an NPC to sell a pylon regardless of happiness if true.")]
        [DefaultValue(true)]
        public bool OverridePylon = true;

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
            if (Main.netMode == NetmodeID.SinglePlayer) return true;
            for (int i = 0; i < Main.maxPlayers; i++)
                if (Netplay.Clients[i].State == 10 && Main.player[i] == Main.player[whoAmI] && Netplay.Clients[i].Socket.GetRemoteAddress().IsLocalHost())
                    return true;

            message = "You are not the server host!";
            return false;
        }
    }
}