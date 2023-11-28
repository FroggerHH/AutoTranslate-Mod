using System.Reflection;

namespace AutoTranslate.Patch;

[HarmonyPatch]
public class RegisterToLocalize
{
    public static Localization checkLocalization1_Russian;
    public static Localization checkLocalization2;
    public static Localization english;

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

    internal static void Init()
    {
        english = new Localization();
        english.SetupLanguage("English");

        checkLocalization1_Russian = new Localization();
        checkLocalization1_Russian.SetupLanguage("Russian");

        checkLocalization2 = new Localization();
        checkLocalization2.SetupLanguage("Swedish");
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    [HarmonyPriority(int.MinValue)]
    private static void Patch()
    {
        Translations.LoadFromFile();

        var onlyEnglishKey = Localization.instance.m_translations.Where(x => OnlyEnglish(x.Key));
        var selectedLanguage = Localization.instance.GetSelectedLanguage();
        foreach (var piece in onlyEnglishKey) Translations.Add(piece.Key, piece.Value, "");

        onlyEnglishKey = english.m_translations.Where(x => OnlyEnglish(x.Key));
        foreach (var piece in onlyEnglishKey) Translations.Add(piece.Key, piece.Value, "");

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
            .Where(x => x != null).Where(NoLocalization<ItemDrop>()).ToList();
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

    public static Func<T, bool> NoLocalization<T>(bool isDescription = false) where T : Component =>
        x =>
        {
            var name = GetName(x, isDescription);
            if (!name.IsGood()) return false;
            return !name.Contains("$");
        };

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

    public static bool StrNoLocalization(string name)
    {
        if (!name.IsGood()) return false;
        if (string.Empty.Equals(Localization.instance.Localize(name))) return true;
        var noLocKey = !name.Contains('$');
        if (noLocKey) return true;
        var onlyEnglish = OnlyEnglish(name);

        return noLocKey || onlyEnglish;
    }

    public static bool OnlyEnglish(string key)
    {
        if (key.Count(x => x == '$') > 1) return false;
        key = key.Replace("$", "");
        if (key.StartsWith("button_") || key.StartsWith("interface_") || key.StartsWith("OLD")
            || key.StartsWith("language_")) return false;
        var keyWithDola = "";
        keyWithDola = $"${key}";
        if (checkLocalization1_Russian == null) return false;
        if (!english.m_translations.ContainsKey(key)) return true;

        var selectedLanguage = Localization.instance.GetSelectedLanguage();
        var selectedTranslation = Localization.instance.Localize(keyWithDola).ToLower();
        if (
            (selectedLanguage != "Russian"
             && selectedTranslation.Equals(checkLocalization1_Russian.Localize(keyWithDola).ToLower()))
            ||
            (selectedLanguage != "Swedish"
             && selectedTranslation.Equals(checkLocalization2.Localize(keyWithDola).ToLower()))
        ) return true;


        return false;
    }

    private static string GetOrigName<T>(T x, bool isDescription = false) where T : Component
    {
        return GetOrigName(GetName(x, isDescription), x.GetPrefabName());
    }

    public static string GetOrigName(string name, string prefabName)
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