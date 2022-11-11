using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.TerrariaDeclassified.Systems.Declassification;

[UsedImplicitly]
public sealed class DamageModifierRedirectionSystem : ModSystem
{
    public override void OnModLoad() {
        base.OnModLoad();

        // Counts as every class.
        On.Terraria.Item.CountsAsClass += (_, _, _) => true;
    }
}