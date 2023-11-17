namespace AutoTranslate.Patch;

[HarmonyPatch]
public class RegisterToLocalize
{
    public static List<Piece> piecesNoName = new();
    public static List<Piece> piecesNoDescription = new();
    public static List<CookingStation> cookingStations = new();
    public static List<CraftingStation> craftingStations = new();
    public static List<Character> creatures = new();
    public static List<ItemDrop> itemsNoName = new();
    public static List<ItemDrop> itemsNoDescription = new();
    public static List<StatusEffect> seNoName = new();
    public static List<StatusEffect> seNoTooltip = new();
    public static List<StatusEffect> seNoStartMessage = new();
    public static List<StatusEffect> seNoStopMessage = new();
    public static List<StatusEffect> seNoRepeatMessage = new();
    public static Localization checkLocalization1;
    public static Localization checkLocalization2;
    public static Localization english;

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    [HarmonyPriority(int.MinValue)]
    private static void Patch()
    {
        Translations.LoadFromFile();
        Translations.originalKeys.Clear();

        if (english == null)
        {
            english = new Localization();
            english.SetupLanguage("English");
        }

        if (checkLocalization1 == null)
        {
            checkLocalization1 = new Localization();
            checkLocalization1.SetupLanguage("Russian");
        }

        if (checkLocalization2 == null)
        {
            checkLocalization2 = new Localization();
            checkLocalization2.SetupLanguage("Swedish");
        }

        piecesNoName = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<Piece>())
            .Where(x => x != null).Where(NoLocalization<Piece>()).ToList();
        piecesNoDescription = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<Piece>())
            .Where(x => x != null).Where(NoLocalization<Piece>(true)).ToList();
        cookingStations = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<CookingStation>())
            .Where(x => x != null).Where(NoLocalization<CookingStation>()).ToList();
        craftingStations = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<CraftingStation>())
            .Where(x => x != null).Where(NoLocalization<CraftingStation>()).ToList();
        creatures = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<Character>())
            .Where(x => x != null).Where(NoLocalization<Character>()).ToList();
        itemsNoName = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<ItemDrop>())
            .Where(x => x != null).Where(NoLocalization<ItemDrop>(false)).ToList();
        itemsNoDescription = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<ItemDrop>())
            .Where(x => x != null).Where(NoLocalization<ItemDrop>(true)).ToList();
        seNoName = ObjectDB.instance.m_StatusEffects.Where(x => StrNoLocalization(x.m_name)).ToList();
        seNoTooltip = ObjectDB.instance.m_StatusEffects.Where(x => StrNoLocalization(x.m_tooltip)).ToList();
        seNoStartMessage = ObjectDB.instance.m_StatusEffects.Where(x => StrNoLocalization(x.m_startMessage)).ToList();
        seNoStopMessage = ObjectDB.instance.m_StatusEffects.Where(x => StrNoLocalization(x.m_stopMessage)).ToList();
        seNoRepeatMessage = ObjectDB.instance.m_StatusEffects.Where(x => StrNoLocalization(x.m_repeatMessage)).ToList();
        foreach (var piece in piecesNoName)
            Translations.Add(Translations.CreateKey(piece), GetOrigName(piece), piece.m_name);
        foreach (var piece in piecesNoDescription)
            Translations.Add(Translations.CreateKey(piece) + "_description", GetOrigName(piece, true),
                piece.m_description);

        foreach (var piece in cookingStations)
            Translations.Add(Translations.CreateKey(piece), GetOrigName(piece), piece.m_name);
        foreach (var piece in craftingStations)
            Translations.Add(Translations.CreateKey(piece), GetOrigName(piece), piece.m_name);
        foreach (var mob in creatures)
            Translations.Add(Translations.CreateKey(mob), GetOrigName(mob), mob.m_name);
        foreach (var item in itemsNoName)
            Translations.Add(Translations.CreateKey(item), GetOrigName(item), item.m_itemData.m_shared.m_name);
        foreach (var item in itemsNoDescription)
            Translations.Add(Translations.CreateKey(item) + "_description", GetOrigName(item, true),
                item.m_itemData.m_shared.m_description);

        foreach (var effect in seNoName)
            Translations.Add(Translations.CreateKey(effect) + "_name", GetOrigName(effect.m_name, effect.name),
                effect.m_name);
        foreach (var effect in seNoTooltip)
            Translations.Add(Translations.CreateKey(effect) + "_tooltip", GetOrigName(effect.m_tooltip, effect.name),
                effect.m_tooltip);
        foreach (var effect in seNoStartMessage)
            Translations.Add(Translations.CreateKey(effect) + "_startMessage",
                GetOrigName(effect.m_startMessage, effect.name), effect.m_startMessage);
        foreach (var effect in seNoStopMessage)
            Translations.Add(Translations.CreateKey(effect) + "_stopMessage",
                GetOrigName(effect.m_stopMessage, effect.name), effect.m_stopMessage);
        foreach (var effect in seNoRepeatMessage)
            Translations.Add(Translations.CreateKey(effect) + "_repeatMessage",
                GetOrigName(effect.m_repeatMessage, effect.name), effect.m_repeatMessage);

        Translations.Update();
    }

    private static Func<T, bool> NoLocalization<T>(bool isDescription = false) where T : Component
    {
        return x =>
            StrNoLocalization(GetName(x, isDescription));
    }

    private static bool StrNoLocalization(string name)
    {
        if (!name.IsGood()) return false;
        if (string.Empty.Equals(Localization.instance.Localize(name))) return true;
        var noLocKey = !name.Contains('$');
        var onlyEnglish = false;
        var keyNoDollar = name.Replace("$", "");
        if (!Localization.instance.m_translations.ContainsKey(keyNoDollar) &&
            Localization.instance.GetSelectedLanguage() == "Russian"
            && !checkLocalization1.m_translations.ContainsKey(keyNoDollar) &&
            Localization.instance.GetSelectedLanguage() == "Swedish"
            && !checkLocalization2.m_translations.ContainsKey(keyNoDollar)) onlyEnglish = true;
        else
        {
            var selectedLanguage = Localization.instance.GetSelectedLanguage();
            var selectedTranslation = Localization.instance.Localize(name).ToLower();
            if ((selectedLanguage != "Russian" && selectedLanguage != "Swedish" &&
                 selectedTranslation.Equals(checkLocalization1.Localize(name).ToLower())) ||
                selectedTranslation.Equals(checkLocalization2.Localize(name).ToLower()))
                onlyEnglish = true;
        }

        return noLocKey || onlyEnglish;
    }

    public static string GetName(Component x, bool isDescription = false)
    {
        return x switch
        {
            Piece piece => isDescription ? piece.m_description : piece.m_name,
            CookingStation cookingStation => cookingStation.m_name,
            CraftingStation craftingStation => craftingStation.m_name,
            Character character => character.m_name,
            ItemDrop itemDrop => isDescription
                ? itemDrop.m_itemData.m_shared.m_description
                : itemDrop.m_itemData.m_shared.m_name,
            _ => string.Empty
        };
    }

    private static string GetOrigName<T>(T x, bool isDescription = false) where T : Component
    {
        return GetOrigName(GetName(x, isDescription), x.GetPrefabName());
    }

    private static string GetOrigName(string name, string prefabName)
    {
        if (!name.IsGood()) return string.Empty;
        if (english.m_translations.ContainsKey(name.Replace("$", "")))
        {
            var localize = english.Localize(name);
            if (localize.IsGood() && !localize.Contains('[')) return localize;
            return prefabName.HumanizeString();
        }

        return name.HumanizeString();
    }
}