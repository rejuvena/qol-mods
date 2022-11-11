using JetBrains.Annotations;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.TerrariaDeclassified.Systems.Declassification;

[UsedImplicitly]
public sealed class TypelessDamageClass : DamageClass
{
    protected override string DisplayNameInternal => Language.GetTextValue("Mods.TerrariaDeclassified.DamageClasses.Typeless");
}