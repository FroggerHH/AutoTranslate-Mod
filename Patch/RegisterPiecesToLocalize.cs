namespace AutoTranslate.Patch;

[HarmonyPatch]
public class RegisterPiecesToLocalize
{
    public static List<Piece> pieces = new();
    public static List<CookingStation> cookingStations = new();
    public static List<CraftingStation> craftingStations = new();
    public static List<Character> creatures = new();
    public static List<ItemDrop> items = new();
    public static Localization checkLocalization1;
    public static Localization checkLocalization2;

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    [HarmonyPriority(int.MinValue)]
    private static void Patch()
    {
        Translations.LoadFromFile();

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

        pieces = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<Piece>())
            .Where(x => x != null).Where(NoLocalization<Piece>()).ToList();
        cookingStations = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<CookingStation>())
            .Where(x => x != null).Where(x => x != null).Where(NoLocalization<CookingStation>()).ToList();
        craftingStations = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<CraftingStation>())
            .Where(x => x != null).Where(x => x != null).Where(NoLocalization<CraftingStation>()).ToList();
        creatures = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<Character>())
            .Where(x => x != null).Where(x => x != null).Where(NoLocalization<Character>()).ToList();
        items = ZNetScene.instance.m_prefabs.Select(x => x.GetComponent<ItemDrop>())
            .Where(x => x != null).Where(x => x != null).Where(NoLocalization<ItemDrop>()).ToList();
        foreach (var piece in pieces)
            Translations.Add(Translations.CreateKey(piece), GetOrigName(piece));
        foreach (var piece in cookingStations)
            Translations.Add(Translations.CreateKey(piece), GetOrigName(piece));
        foreach (var piece in craftingStations)
            Translations.Add(Translations.CreateKey(piece), GetOrigName(piece));
        foreach (var mob in creatures)
            Translations.Add(Translations.CreateKey(mob), GetOrigName(mob));
        foreach (var item in items)
        {
            Translations.Add(Translations.CreateKey(item), GetOrigName(item));
            Translations.Add(Translations.CreateKey(item) + "_description",
                item.m_itemData.m_shared.m_description.HumalateString());
        }

        Translations.Update();
    }

    private static Func<T, bool> NoLocalization<T>() where T : Component { return x => StrNoLocalization(GetName(x)); }

    private static bool StrNoLocalization(string name)
    {
        var noLocKey = !name.Contains('$');
        var onlyEnglish = false;
        var keyNoDollar = name.Replace("$", "");
        if (!Localization.instance.m_translations.ContainsKey(keyNoDollar) ||
            !checkLocalization1.m_translations.ContainsKey(keyNoDollar) ||
            !checkLocalization2.m_translations.ContainsKey(keyNoDollar))
        {
            onlyEnglish = true;
        } else
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

    public static string GetName(Component x)
    {
        return x switch
        {
            Piece piece => piece.m_name,
            CookingStation cookingStation => cookingStation.m_name,
            CraftingStation craftingStation => craftingStation.m_name,
            Character character => character.m_name,
            ItemDrop itemDrop => itemDrop.m_itemData.m_shared.m_name,
            _ => string.Empty
        };
    }

    private static string GetOrigName<T>(T x) where T : Component
    {
        var name = GetName(x);
        return name.IsGood() ? name.HumalateString() : x.GetPrefabName().HumalateString();
    }
}