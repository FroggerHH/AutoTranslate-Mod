using AutoTranslate.Patch;
using RavSoft.GoogleTranslator;

namespace AutoTranslate;

public static class Translations
{
    private static Dictionary<string, Dictionary<string, string>> _all;
    internal static Dictionary<string, string> originalKeys = new();

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
        if (dictionary.ContainsKey(key))
            //DebugError($"Key {key} already exists");
            return;

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

    public static void Update()
    {
        var watch = StartNew();
        var counter = 0;
        var loadedFromFileCounter = 0;
        DebugWarning("Starting localizing mods. Be patient, it would take a while. Like even a couple of minutes.",
            false);

        var selectedLanguage = Localization.instance.GetSelectedLanguage();
        var translations = GetAll().First();
        var selectedTranslation = GetAll()[selectedLanguage]!;

        foreach (var pair in translations.Value)
        {
            var localizedWord = string.Empty;
            var key = pair.Key;
            if (selectedTranslation.TryGetValue(key, out var savedWord))
            {
                localizedWord = savedWord;
                loadedFromFileCounter++;
            } else
            {
                var flag = true;
                if (originalKeys.TryGetValue(key, out var originalKey))
                    if (Localization.instance.m_translations.ContainsKey(originalKey))
                    {
                        localizedWord = Localization.instance.Localize(originalKey);
                        flag = false;
                    }

                if (flag)
                {
                    localizedWord = LocalizeWord(pair.Value, key, selectedLanguage);
                    if (localizedWord.Equals("en")) continue;
                    if (GoogleTranslator.Instance.Error != null)
                        DebugError($"Translation error: {GoogleTranslator.Instance.Error.Message}");

                    if (showTranslationLogs.Value)
                        Debug($"Translated key='{key}', word='{pair.Value}', localized='{localizedWord}'");
                    counter++;
                }
            }

            Localization.instance.AddWord(key, localizedWord);
        }

        ApplyLocalization.Apply();

        SaveToFile();
        watch.Stop();
        Debug($"Done localizing in {watch.Elapsed}. Translated {counter} words. "
              + $"Loaded from file {loadedFromFileCounter} words.");
    }

    private static string LocalizeWord(string word, string key, string language)
    {
        var localizedWord = GoogleTranslator.Instance.Translate(word, "English", language);
        GetAll()[language][key] = localizedWord;
        return localizedWord;
    }

    public static string CreateKey(Object obj) { return $"{obj.GetPrefabName()}___{ModName}"; }
}