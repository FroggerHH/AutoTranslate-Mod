using AutoTranslate.Patch;
using RavSoft.GoogleTranslator;

namespace AutoTranslate;

public static class Translations
{
    private static Dictionary<string, Dictionary<string, string>> _all;
    internal static Dictionary<string, string> originalKeys = new();
    internal static GameObject menuRoot;
    internal static TextMeshProUGUI menuText;

    private static int translateCounter;

    private static string textMessageTemplate;

    public static Dictionary<string, Dictionary<string, string>> GetAll()
    {
        if (_all == null)
        {
            _all = new Dictionary<string, Dictionary<string, string>>();

            _all.Add("English", new Dictionary<string, string>());
            _all.Add("Swedish", new Dictionary<string, string>());
            _all.Add("French", new Dictionary<string, string>());
            _all.Add("Italian", new Dictionary<string, string>());
            _all.Add("German", new Dictionary<string, string>());
            _all.Add("Spanish", new Dictionary<string, string>());
            _all.Add("Russian", new Dictionary<string, string>());
            _all.Add("Romanian", new Dictionary<string, string>());
            _all.Add("Bulgarian", new Dictionary<string, string>());
            _all.Add("Macedonian", new Dictionary<string, string>());
            _all.Add("Finnish", new Dictionary<string, string>());
            _all.Add("Danish", new Dictionary<string, string>());
            _all.Add("Norwegian", new Dictionary<string, string>());
            _all.Add("Icelandic", new Dictionary<string, string>());
            _all.Add("Turkish", new Dictionary<string, string>());
            _all.Add("Lithuanian", new Dictionary<string, string>());
            _all.Add("Czech", new Dictionary<string, string>());
            _all.Add("Hungarian", new Dictionary<string, string>());
            _all.Add("Slovak", new Dictionary<string, string>());
            _all.Add("Polish", new Dictionary<string, string>());
            _all.Add("Dutch", new Dictionary<string, string>());
            _all.Add("Chinese", new Dictionary<string, string>());
            _all.Add("Chinese_Trad", new Dictionary<string, string>());
            _all.Add("Japanese", new Dictionary<string, string>());
            _all.Add("Korean", new Dictionary<string, string>());
            _all.Add("Hindi", new Dictionary<string, string>());
            _all.Add("Thai", new Dictionary<string, string>());
            _all.Add("Abenaki", new Dictionary<string, string>());
            _all.Add("Croatian", new Dictionary<string, string>());
            _all.Add("Georgian", new Dictionary<string, string>());
            _all.Add("Greek", new Dictionary<string, string>());
            _all.Add("Serbian", new Dictionary<string, string>());
            _all.Add("Ukrainian", new Dictionary<string, string>());
            _all.Add("Latvian", new Dictionary<string, string>());
        }

        return _all;
    }

    public static void Add(string key, string value, string originalKey)
    {
        if (string.Empty.Equals(key)) return;
        var dictionary = GetAll()["English"];
        originalKeys.Remove(key);
        originalKeys.Add(key, originalKey);
        dictionary.Remove(key);
        dictionary.Add(key, value);
    }

    public static void LoadFromFile()
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!File.Exists(filePath)) return;

        using (var reader = new StreamReader(filePath))
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

            var deserialize = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(reader);
            var all = GetAll();
            foreach (var pair in deserialize)
            {
                all.Remove(pair.Key);
                all.Add(pair.Key, pair.Value);
            }
        }
    }

    public static void SaveToFile()
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        using (var writer = new StreamWriter(filePath, false))
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

            serializer.Serialize(writer, _all);
        }
    }

    public static async void Update()
    {
        var watch = StartNew();
        translateCounter = 1;
        var updateProgressCounter = 0;
        var showMenuCounter = 0;
        DebugWarning("Starting localizing mods." + " Be patient, it would take a while. Like even a couple of minutes.",
            false);

        var selectedLanguage = Localization.instance.GetSelectedLanguage();
        var translations = GetAll().First();
        var selectedTranslation = GetAll()[selectedLanguage]!;

        UpdateMenuText(1, translations.Value.Count);
        await Task.Delay(100);
        foreach (var pair in translations.Value)
        {
            updateProgressCounter++;
            showMenuCounter++;
            if (updateProgressCounter >= 15)
            {
                UpdateMenuText(translateCounter, translations.Value.Count);
                updateProgressCounter = 0;
            }

            if (showMenuCounter >= 100)
            {
                await Task.Delay(50);
                showMenuCounter = 0;
            }

            ProgressWord(pair, selectedTranslation, selectedLanguage);
        }

        ApplyLocalization.Apply();
        SaveToFile();
        watch.Stop();
        Debug($"Done localizing in {watch.Elapsed}. Translated {translateCounter} words. ");
        translateCounter = 0;
        UpdateMenuText(0, 0);
    }

    private static void ProgressWord(KeyValuePair<string, string> pair, Dictionary<string, string> selectedTranslation,
        string selectedLanguage)
    {
        string localizedWord;
        var key = pair.Key;
        if (selectedTranslation.TryGetValue(key, out var savedWord))
        {
            localizedWord = savedWord;
        } else
        {
            if (originalKeys.TryGetValue(key, out var originalKey)
                && Localization.instance.m_translations.ContainsKey(originalKey))
            {
                localizedWord = Localization.instance.Localize(originalKey);
            } else
            {
                localizedWord = LocalizeWord(pair.Value, key, selectedLanguage);
                if (localizedWord.Equals("en")) localizedWord = pair.Value;
                if (GoogleTranslator.Instance.Error != null)
                    DebugError($"Translation error: {GoogleTranslator.Instance.Error.Message}");

                if (showTranslationLogs.Value)
                    Debug($"Translated key='{key}', word='{pair.Value}', localized='{localizedWord}'");
            }
        }

        Localization.instance.AddWord(key, localizedWord);
        translateCounter++;
    }

    internal static void UpdateMenuText(int doneCounter, int allCount)
    {
        if (!textMessageTemplate.IsGood())
            textMessageTemplate = GoogleTranslator.Instance.Translate(
                "Идёт перевод...\n Переведено {0}/{1} слов.", "Russian", Localization.instance.GetSelectedLanguage());

        if (menuRoot != null)
        {
            menuRoot.SetActive(doneCounter > 0);
            menuText.text = string.Format(textMessageTemplate, doneCounter, allCount);
        }
    }

    private static string LocalizeWord(string word, string key, string language)
    {
        var localizedWord = GoogleTranslator.Instance.Translate(word, "English", language);
        GetAll()[language][key] = localizedWord;
        return localizedWord;
    }

    public static string CreateKey(Object obj) => $"{obj.GetPrefabName()}___{ModName}";
}