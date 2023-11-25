using System.Reflection;

namespace AutoTranslate.Patch;

[HarmonyPatch]
public static class ApplyLocalization
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    public static void Apply()
    {
        foreach (var piece in RegisterToLocalize.piecesNoName) piece.m_name = $"${Translations.CreateKey(piece)}";
        foreach (var piece in RegisterToLocalize.piecesNoDescription)
            piece.m_description = $"${Translations.CreateKey(piece)}_description";

        foreach (var piece in RegisterToLocalize.cookingStations)
            piece.m_name = $"${Translations.CreateKey(piece)}";
        foreach (var piece in RegisterToLocalize.craftingStations)
            piece.m_name = $"${Translations.CreateKey(piece)}";
        foreach (var mob in RegisterToLocalize.creatures)
            mob.m_name = $"${Translations.CreateKey(mob)}";
        foreach (var item in RegisterToLocalize.itemsNoName)
            item.m_itemData.m_shared.m_name = $"${Translations.CreateKey(item)}";
        foreach (var item in RegisterToLocalize.itemsNoDescription)
            item.m_itemData.m_shared.m_description = $"${Translations.CreateKey(item)}_description";

        foreach (var effect in RegisterToLocalize.seNoName)
            effect.m_name = $"${Translations.CreateKey(effect)}_name";
        foreach (var effect in RegisterToLocalize.seNoTooltip)
            effect.m_tooltip = $"${Translations.CreateKey(effect)}_tooltip";
        foreach (var effect in RegisterToLocalize.seNoStartMessage)
            effect.m_startMessage = $"${Translations.CreateKey(effect)}_startMessage";
        foreach (var effect in RegisterToLocalize.seNoStopMessage)
            effect.m_stopMessage = $"${Translations.CreateKey(effect)}_stopMessage";
        foreach (var effect in RegisterToLocalize.seNoRepeatMessage)
            effect.m_repeatMessage = $"${Translations.CreateKey(effect)}_repeatMessage";

        foreach (var data in RegisterCustomHover.hoverableDatas)
            data.field.SetValue(data.component, $"${Translations.CreateKey(data.prefab)}__field_{data.field.Name}");
    }
}