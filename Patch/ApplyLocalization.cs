namespace AutoTranslate.Patch;

[HarmonyPatch]
public static class ApplyLocalization
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    public static void Apply()
    {
        foreach (var piece in RegisterPiecesToLocalize.pieces)
            piece.m_name = $"${Translations.CreateKey(piece)}";
        foreach (var piece in RegisterPiecesToLocalize.cookingStations)
            piece.m_name = $"${Translations.CreateKey(piece)}";
        foreach (var piece in RegisterPiecesToLocalize.craftingStations)
            piece.m_name = $"${Translations.CreateKey(piece)}";
        foreach (var mob in RegisterPiecesToLocalize.creatures)
            mob.m_name = $"${Translations.CreateKey(mob)}";
        foreach (var item in RegisterPiecesToLocalize.items)
        {
            item.m_itemData.m_shared.m_name = $"${Translations.CreateKey(item)}";
            item.m_itemData.m_shared.m_description = $"${Translations.CreateKey(item)}_description";
        }
    }
}