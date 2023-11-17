using BepInEx.Configuration;

namespace AutoTranslate;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string
        ModName = "AutoTranslate",
        ModVersion = "1.1.0",
        ModAuthor = "Frogger",
        ModGUID = $"com.{ModAuthor}.{ModName}";

    internal static ConfigEntry<bool> showTranslationLogs;

    public static readonly string folderPath = Path.Combine(Paths.BepInExRootPath, $"{ModName}-Translations");
    public static readonly string filePath = Path.Combine(folderPath, "AutoLocalization.yml");

    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        showTranslationLogs = config("Debug", "ShowTranslationLogs", false, "Show how translations are generated.");

        Localization.OnLanguageChange += Translations.Update;
    }
}