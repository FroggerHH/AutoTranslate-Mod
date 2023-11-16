namespace AutoTranslate.Patch;

[HarmonyPatch]
public static class Init
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))] [HarmonyPostfix] [HarmonyWrapSafe]
    private static void Patch(FejdStartup __instance) { Translations.Init(); }
}