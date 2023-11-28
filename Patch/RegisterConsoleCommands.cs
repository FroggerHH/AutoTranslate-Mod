namespace AutoTranslate.Patch;

[HarmonyPatch(typeof(Terminal))]
public class RegisterConsoleCommands
{
    public static Dictionary<string, ConsoleCommand> commands = new();

    [HarmonyPatch(nameof(InitTerminal))] [HarmonyPostfix] [HarmonyWrapSafe]
    [HarmonyPriority(int.MinValue)]
    private static void PatchCommands()
    {
        foreach (var pair in Terminal.commands)
        {
            var command = pair.Value;
            var commandName = pair.Key;
            if (commands.ContainsKey(commandName) || commands.ContainsValue(command)) continue;
            if (!command.Description.IsGood()) continue;
            if (!RegisterToLocalize.StrNoLocalization(command.Description)) continue;
            commands.Add(commandName, command);
            var key = $"{commandName}___{ModName}_ConsoleCommand";
            Translations.Add(key, command.Description, "");
        }
    }

    [HarmonyPatch]
    private static class PatchAddString
    {
        [HarmonyPatch(nameof(Terminal.AddString), typeof(string))]
        private static void Prefix1(string text) { }
    }
}