using Terraria;
using Terraria.ModLoader;

namespace HappinessRemoval
{
    public class HappinessPlayer : ModPlayer
    {
        public override void PreUpdate()
        {
            if (ModContent.GetInstance<HappinessConfig>().ToggleHappiness) return;

            Player.currentShoppingSettings.PriceAdjustment = ModContent.GetInstance<HappinessConfig>().NpcHappiness;

            if (!Main.npcChatFocus4)
                return;

            Main.instance.MouseText("NPC happiness disabled by NPC Happiness Removal!");
            Main.mouseText = true;
        }
    }
}