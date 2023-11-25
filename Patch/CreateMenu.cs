using UnityEngine.UI;

namespace AutoTranslate.Patch;

[HarmonyPatch]
public class CreateMenu
{
    public static Transform vanilaCanvas;
    public static GameObject canvas;

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))] [HarmonyPostfix] [HarmonyWrapSafe]
    private static void Create()
    {
        if (Translations.menuRoot) return;
        var fs = FejdStartup.instance;
        var original = fs.transform.FindChildByName("PleaseWait").gameObject;

        vanilaCanvas = original.transform.parent.parent;
        canvas = new GameObject();
        DontDestroyOnLoad(canvas);
        var canvasComp = canvas.AddComponentCopy(vanilaCanvas.GetComponent<Canvas>());
        canvasComp.sortingOrder++;
        canvas.AddComponentCopy(vanilaCanvas.GetComponent<CanvasScaler>());
        canvas.AddComponentCopy(vanilaCanvas.GetComponent<GraphicRaycaster>());
        canvas.AddComponent<GuiScaler>();
        canvas.name = "Canvas for AutoTranslate";

        var newPanel = Instantiate(original, canvas.transform, true);
        var text = newPanel.GetComponentInChildren<TextMeshProUGUI>();
        text.text = "NONE";
        text.autoSizeTextContainer = true;
        newPanel.SetActive(false);

        Translations.menuRoot = newPanel;
        Translations.menuText = text;
    }
}