using System.Reflection;

namespace AutoTranslate.Patch;

[HarmonyPatch]
public static class ApplyLocalization
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix]
    public static void Apply()
    {
        var log = "";
        try
        {
            log += "0, ";
            foreach (var piece in RegisterToLocalize.piecesNoName) piece.m_name = $"${Translations.CreateKey(piece)}";
            log += "1, ";
            foreach (var piece in RegisterToLocalize.piecesNoDescription)
                piece.m_description = $"${Translations.CreateKey(piece)}_description";
            log += "2, ";
            foreach (var piece in RegisterToLocalize.cookingStations)
                piece.m_name = $"${Translations.CreateKey(piece)}";
            log += "3, ";
            foreach (var piece in RegisterToLocalize.craftingStations)
                piece.m_name = $"${Translations.CreateKey(piece)}";
            log += "4, ";
            foreach (var mob in RegisterToLocalize.creatures)
                mob.m_name = $"${Translations.CreateKey(mob)}";
            log += "5, ";
            foreach (var item in RegisterToLocalize.itemsNoName)
                item.m_itemData.m_shared.m_name = $"${Translations.CreateKey(item)}";
            log += "6, ";
            foreach (var item in RegisterToLocalize.itemsNoDescription)
                item.m_itemData.m_shared.m_description = $"${Translations.CreateKey(item)}_description";
            log += "7, ";

            foreach (var effect in RegisterToLocalize.seNoName)
                effect.m_name = $"${Translations.CreateKey(effect)}_name";
            log += "8, ";
            foreach (var effect in RegisterToLocalize.seNoTooltip)
                effect.m_tooltip = $"${Translations.CreateKey(effect)}_tooltip";
            log += "9, ";
            foreach (var effect in RegisterToLocalize.seNoStartMessage)
                effect.m_startMessage = $"${Translations.CreateKey(effect)}_startMessage";
            log += "10, ";
            foreach (var effect in RegisterToLocalize.seNoStopMessage)
                effect.m_stopMessage = $"${Translations.CreateKey(effect)}_stopMessage";
            log += "11, ";
            foreach (var effect in RegisterToLocalize.seNoRepeatMessage)
                effect.m_repeatMessage = $"${Translations.CreateKey(effect)}_repeatMessage";
            log += "12, ";

            foreach (var data in RegisterCustomHover.hoverableDatas)
                data.field.SetValue(data.component, $"${Translations.CreateKey(data.prefab)}__field_{data.field.Name}");
            log += "13, ";

            foreach (var pair in RegisterConsoleCommands.commands)
            {
                var command = pair.Value;
                var commandName = pair.Key;
                command.Description =
                    $"${commandName}___{ModName}_ConsoleCommand".Localize();
            }

            log += "done";
        }
        catch (Exception e)
        {
            DebugError($"ApplyLocalization error: {e}\n{log}");
        }
    }
}