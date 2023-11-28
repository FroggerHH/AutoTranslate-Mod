using System.Reflection;
using static AutoTranslate.Patch.RegisterToLocalize;

namespace AutoTranslate.Patch;

[HarmonyPatch]
public class RegisterCustomHover
{
    public static List<HoverableData> hoverableDatas = new();

    [UsedImplicitly]
    private static List<HoverableData> test = new();

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    [HarmonyPriority(int.MinValue)]
    private static void Patch()
    {
        List<HoverableData> noFilter = new();
        foreach (var prefab in ZNetScene.instance.m_prefabs)
        {
            var component = prefab?.GetComponent<Hoverable>();
            if (component is null || component is ItemDrop) continue;
            if (component is MineRock mineRock)
            {
                if (mineRock.gameObject != null)
                {
                    var m_hitAreas = mineRock.m_hitAreas = mineRock.m_areaRoot != null
                        ? mineRock.m_areaRoot.GetComponentsInChildren<Collider>()
                        : mineRock.gameObject.GetComponentsInChildren<Collider>() ?? new Collider[0];

                    foreach (var hitArea in m_hitAreas)
                    {
                        var hoverable = hitArea?.GetComponent<Hoverable>();
                        if (hoverable is null) continue;
                        noFilter.Add(new HoverableData(hoverable, hitArea.gameObject, GetNameField(hoverable)));
                    }
                }
            }


            foreach (var field in GetAllFields(component)) noFilter.Add(new HoverableData(component, prefab, field));
        }

        noFilter = noFilter.Where(x => x != null).ToList();

        hoverableDatas = noFilter.Distinct().Where(x =>
        {
            var value = x.field?.GetValue(x.component)?.ToString();
            if (value is null) return false;
            return StrNoLocalization(value);
        }).ToList();


        test = hoverableDatas.FindAll(x => x.prefab.name.Contains("Tar") && x.prefab.name.Contains("Collector"));

        foreach (var data in hoverableDatas)
        {
            string originalKey = data.field.GetValue(data.component)?.ToString();
            Translations.Add($"{Translations.CreateKey(data.prefab)}__field_{data.field.Name}",
                GetOrigName(originalKey, data.prefab.GetPrefabName()), "");
        }
    }

    [CanBeNull]
    public static FieldInfo GetNameField(Hoverable x)
    {
        if (x is null) return null;
        var type = x.GetType();
        FieldInfo value;
        if (type == typeof(HoverText))
            value = type.GetField("m_text", BindingFlags.Public | BindingFlags.Instance);
        else
        {
            value = type.GetField("m_name", BindingFlags.NonPublic | BindingFlags.Instance);
            if (value is null) value = type.GetField("m_name", BindingFlags.Public | BindingFlags.Instance);
        }

        if (value is null) return null;
        return value;
    }

    public static FieldInfo[] GetAllFields(Hoverable hoverable)
    {
        if (hoverable is null) return null;
        var type = hoverable.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).ToList();
        fields.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
        fields = fields.Where(x => x.FieldType == typeof(string)).Distinct().ToList();
        return fields
            .Where(x => x is not null)
            .Where(x =>
            {
                var value = x.GetValue(hoverable);
                if (value is null) return false;
                return RegisterToLocalize.OnlyEnglish(value.ToString());
            })
            .ToArray();
    }

    public static string GetName(Hoverable x)
    {
        var value = GetNameField(x)?.GetValue(x);
        if (value is null) return string.Empty;
        return value.ToString();
    }

    public class HoverableData
    {
        [NotNull] public Hoverable component;
        public GameObject prefab;
        public FieldInfo field;

        public HoverableData(Hoverable component, GameObject prefab, FieldInfo field)
        {
            this.component = component;
            this.prefab = prefab;
            this.field = field;
        }

        public override string ToString()
        {
            return
                $"Component: '{component.ToString() ?? "null"}', "
                + $"Prefab: '{prefab?.name ?? "null"}', "
                + $"Field: '{field?.Name ?? "null"}'";
        }
    }
}