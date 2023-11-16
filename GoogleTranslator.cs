// Copyright (c) 2015 Ravi Bhavnani
// License: Code Project Open License
// http://www.codeproject.com/info/cpol10.aspx

using System.Net;

// ReSharper disable once CheckNamespace
namespace RavSoft.GoogleTranslator;

/// <summary>
///     Translates text using Google's online language tools.
/// </summary>
public class GoogleTranslator
{
    #region Fields

    /// <summary>
    ///     The language to translation mode map.
    /// </summary>
    private static Dictionary<string, string> _languageModeMap;

    #endregion

    #region Public methods

    /// <summary>
    ///     Translates the specified source text.
    /// </summary>
    /// <param name="sourceText">The source text.</param>
    /// <param name="sourceLanguage">The source language.</param>
    /// <param name="targetLanguage">The target language.</param>
    /// <returns>The translation.</returns>
    public string Translate(string sourceText, string sourceLanguage, string targetLanguage)
    {
        // Initialize
        Error = null;
        TranslationSpeechUrl = null;
        TranslationTime = TimeSpan.Zero;
        var tmStart = DateTime.Now;
        var translation = string.Empty;

        try
        {
            // Download translation
            var url = string.Format(
                "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
                LanguageEnumToIdentifier(sourceLanguage),
                LanguageEnumToIdentifier(targetLanguage),
                // HttpUtility.UrlEncode(sourceText));
                Uri.EscapeDataString(sourceText));
            var outputFile = Path.GetTempFileName();
            using (var wc = new WebClient())
            {
                wc.Headers.Add("user-agent",
                    "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                wc.DownloadFile(url, outputFile);
            }

            // Get translated text
            if (File.Exists(outputFile))
            {
                // Get phrase collection
                var text = File.ReadAllText(outputFile);
                var index = text.IndexOf($",,\"{LanguageEnumToIdentifier(sourceLanguage)}\"",
                    StringComparison.Ordinal);
                if (index == -1)
                {
                    // Translation of single word
                    var startQuote = text.IndexOf('\"');
                    if (startQuote != -1)
                    {
                        var endQuote = text.IndexOf('\"', startQuote + 1);
                        if (endQuote != -1) translation = text.Substring(startQuote + 1, endQuote - startQuote - 1);
                    }
                } else
                {
                    // Translation of phrase
                    text = text.Substring(0, index);
                    text = text.Replace("],[", ",");
                    text = text.Replace("]", string.Empty);
                    text = text.Replace("[", string.Empty);
                    text = text.Replace("\",\"", "\"");

                    // Get translated phrases
                    string[] phrases = text.Split(new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < phrases.Count(); i += 2)
                    {
                        var translatedPhrase = phrases[i];
                        if (translatedPhrase.StartsWith(",,"))
                        {
                            i--;
                            continue;
                        }

                        translation += translatedPhrase + "  ";
                    }
                }

                // Fix up translation
                translation = translation.Trim();
                translation = translation.Replace(" ?", "?");
                translation = translation.Replace(" !", "!");
                translation = translation.Replace(" ,", ",");
                translation = translation.Replace(" .", ".");
                translation = translation.Replace(" ;", ";");

                // And translation speech URL
                TranslationSpeechUrl = string.Format(
                    "https://translate.googleapis.com/translate_tts?ie=UTF-8&q={0}&tl={1}&total=1&idx=0&textlen={2}&client=gtx",
                    Uri.EscapeDataString(translation), LanguageEnumToIdentifier(targetLanguage),
                    // HttpUtility.UrlEncode(translation), LanguageEnumToIdentifier(targetLanguage),
                    translation.Length);
            }
        }
        catch (Exception ex)
        {
            Error = ex;
        }

        // Return result
        TranslationTime = DateTime.Now - tmStart;
        return translation;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the supported languages.
    /// </summary>
    public static IEnumerable<string> Languages
    {
        get
        {
            EnsureInitialized();
            return _languageModeMap.Keys.OrderBy(p => p);
        }
    }

    /// <summary>
    ///     Gets the time taken to perform the translation.
    /// </summary>
    public TimeSpan TranslationTime { get; private set; }

    /// <summary>
    ///     Gets the url used to speak the translation.
    /// </summary>
    /// <value>The url used to speak the translation.</value>
    public string TranslationSpeechUrl { get; private set; }

    /// <summary>
    ///     Gets the error.
    /// </summary>
    public Exception Error { get; private set; }

    private static GoogleTranslator _instance;

    public static GoogleTranslator Instance
    {
        get
        {
            _instance ??= new GoogleTranslator();
            return _instance;
        }
    }

    #endregion

    #region Private methods

    /// <summary>
    ///     Converts a language to its identifier.
    /// </summary>
    /// <param name="language">The language."</param>
    /// <returns>The identifier or <see cref="string.Empty" /> if none.</returns>
    private static string LanguageEnumToIdentifier(string language)
    {
        string mode;
        EnsureInitialized();
        _languageModeMap.TryGetValue(language, out mode);
        return mode;
    }

    /// <summary>
    ///     Ensures the translator has been initialized.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_languageModeMap == null)
        {
            _languageModeMap = new Dictionary<string, string>();
            _languageModeMap.Add("Afrikaans", "af");
            _languageModeMap.Add("Albanian", "sq");
            _languageModeMap.Add("Arabic", "ar");
            _languageModeMap.Add("Armenian", "hy");
            _languageModeMap.Add("Azerbaijani", "az");
            _languageModeMap.Add("Basque", "eu");
            _languageModeMap.Add("Belarusian", "be");
            _languageModeMap.Add("Bengali", "bn");
            _languageModeMap.Add("Bulgarian", "bg");
            _languageModeMap.Add("Catalan", "ca");
            _languageModeMap.Add("Chinese", "zh-CN");
            _languageModeMap.Add("Croatian", "hr");
            _languageModeMap.Add("Czech", "cs");
            _languageModeMap.Add("Danish", "da");
            _languageModeMap.Add("Dutch", "nl");
            _languageModeMap.Add("English", "en");
            _languageModeMap.Add("Esperanto", "eo");
            _languageModeMap.Add("Estonian", "et");
            _languageModeMap.Add("Filipino", "tl");
            _languageModeMap.Add("Finnish", "fi");
            _languageModeMap.Add("French", "fr");
            _languageModeMap.Add("Galician", "gl");
            _languageModeMap.Add("German", "de");
            _languageModeMap.Add("Georgian", "ka");
            _languageModeMap.Add("Greek", "el");
            _languageModeMap.Add("Haitian Creole", "ht");
            _languageModeMap.Add("Hebrew", "iw");
            _languageModeMap.Add("Hindi", "hi");
            _languageModeMap.Add("Hungarian", "hu");
            _languageModeMap.Add("Icelandic", "is");
            _languageModeMap.Add("Indonesian", "id");
            _languageModeMap.Add("Irish", "ga");
            _languageModeMap.Add("Italian", "it");
            _languageModeMap.Add("Japanese", "ja");
            _languageModeMap.Add("Korean", "ko");
            _languageModeMap.Add("Lao", "lo");
            _languageModeMap.Add("Latin", "la");
            _languageModeMap.Add("Latvian", "lv");
            _languageModeMap.Add("Lithuanian", "lt");
            _languageModeMap.Add("Macedonian", "mk");
            _languageModeMap.Add("Malay", "ms");
            _languageModeMap.Add("Maltese", "mt");
            _languageModeMap.Add("Norwegian", "no");
            _languageModeMap.Add("Persian", "fa");
            _languageModeMap.Add("Polish", "pl");
            _languageModeMap.Add("Portuguese", "pt");
            _languageModeMap.Add("Romanian", "ro");
            _languageModeMap.Add("Russian", "ru");
            _languageModeMap.Add("Serbian", "sr");
            _languageModeMap.Add("Slovak", "sk");
            _languageModeMap.Add("Slovenian", "sl");
            _languageModeMap.Add("Spanish", "es");
            _languageModeMap.Add("Swahili", "sw");
            _languageModeMap.Add("Swedish", "sv");
            _languageModeMap.Add("Tamil", "ta");
            _languageModeMap.Add("Telugu", "te");
            _languageModeMap.Add("Thai", "th");
            _languageModeMap.Add("Turkish", "tr");
            _languageModeMap.Add("Ukrainian", "uk");
            _languageModeMap.Add("Urdu", "ur");
            _languageModeMap.Add("Vietnamese", "vi");
            _languageModeMap.Add("Welsh", "cy");
            _languageModeMap.Add("Yiddish", "yi");
        }
    }

    #endregion
}