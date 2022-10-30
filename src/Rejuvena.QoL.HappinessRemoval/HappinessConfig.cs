using System.ComponentModel;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace Rejuvena.QoL.HappinessRemoval
{
    [UsedImplicitly] [Label("$Mods.HappinessRemoval.Config.ConfigName")]
    public sealed class HappinessConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [UsedImplicitly]
        [Header("$Mods.HappinessRemoval.Config.HappinessHeader")]
        [Label("$Mods.HappinessRemoval.Config.ToggleHappinessLabel")]
        [Tooltip("$Mods.HappinessRemoval.Config.ToggleHappinessTooltip")]
        [DefaultValue(false)]
        public bool ToggleHappiness { get; set; }

        [UsedImplicitly]
        [Label("$Mods.HappinessRemoval.Config.NpcHappinessLabel")]
        [Tooltip("$Mods.HappinessRemoval.Config.NpcHappinessTooltip")]
        [Slider]
        [DefaultValue(0.75f)]
        [Range(0.5f, 2f)]
        public float NpcHappiness { get; set; }

        [UsedImplicitly]
        [Label("$Mods.HappinessRemoval.Config.OverridePylonLabel")]
        [Tooltip("$Mods.HappinessRemoval.Config.OverridePylonTooltip")]
        [DefaultValue(true)]
        public bool OverridePylon { get; set; }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
            if (Main.netMode == NetmodeID.SinglePlayer) return true;
            for (int i = 0; i < Main.maxPlayers; i++)
                if (Netplay.Clients[i].State == 10 && Main.player[i] == Main.player[whoAmI] && Netplay.Clients[i].Socket.GetRemoteAddress().IsLocalHost())
                    return true;

            message = Language.GetTextValue("Mods.HappinessRemoval.Config.NotTheServerHost");
            return false;
        }
    }
}