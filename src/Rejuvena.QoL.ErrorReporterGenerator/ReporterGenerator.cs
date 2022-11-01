using Microsoft.CodeAnalysis;

namespace Rejuvena.QoL.ErrorReporterGenerator;

[Generator]
public class ReporterGenerator : ISourceGenerator
{
    void ISourceGenerator.Initialize(GeneratorInitializationContext context) {
    }

    void ISourceGenerator.Execute(GeneratorExecutionContext context) {
        string asmName = context.Compilation.AssemblyName ?? "<unknown>";

        string source = @$"
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace {asmName};

[JetBrains.Annotations.UsedImplicitly]
[System.Runtime.CompilerServices.CompilerGenerated]
internal sealed class ErrorReporterSystem : ModSystem
{{
    public List<string> Warnings {{ get; }} = new();
}}

[JetBrains.Annotations.UsedImplicitly]
[System.Runtime.CompilerServices.CompilerGenerated]
internal sealed class ErrorReporterPlayer : ModPlayer
{{
    private ErrorReporterSystem ErrorReporter => ModContent.GetInstance<ErrorReporterSystem>();

    public override void OnEnterWorld(Player player) {{
        static void Log(string text) {{
            Main.NewText(""WARNING: "" + text, Colors.RarityRed);
        }}

        if (ErrorReporter.Warnings.Count == 0) return;

        Log(""Non-fatal errors occurred during the loading process of 'Happiness Removal'."");
        Log(""Please send the logs to the mod's developer through their homepage."");
        Log(""A list of caught errors are as follows:"");
        ErrorReporter.Warnings.ForEach(Log);
    }}
}}

[JetBrains.Annotations.UsedImplicitly]
[System.Runtime.CompilerServices.CompilerGenerated]
internal static class ErrorReporterExtensions
{{
    public static void AddWarning(this Mod mod, string warning) {{
        ModContent.GetInstance<ErrorReporterSystem>().Warnings.Add(warning);
    }}
}}
".Trim();
        
        context.AddSource("ErrorReporter.g.cs", source);
    }
}