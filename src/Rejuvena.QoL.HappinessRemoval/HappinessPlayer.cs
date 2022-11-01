using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.QoL.HappinessRemoval;

public class HappinessPlayer : ModPlayer
{
    public override void PreUpdate()
    {
        if (ModContent.GetInstance<HappinessConfig>().ToggleHappiness) return;

        Player.currentShoppingSettings.PriceAdjustment = ModContent.GetInstance<HappinessConfig>().NpcHappiness;

        if (!Main.npcChatFocus4)
            return;

        Main.instance.MouseText(Language.GetTextValue("Mods.HappinessRemoval.Chat.HappinessRemoved"));
        Main.mouseText = true;
    }
}