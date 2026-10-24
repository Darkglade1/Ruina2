using System.Collections;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Ruina2.Ruina2Code.Patches;

// A10 double boss breaks Blue Scarf unless I manually clean this shit up
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
public static class NMapScreenPatch
{
    private static FieldInfo[]? _fields;

    [HarmonyPrefix]
    private static void CleanStuff(NMapScreen __instance, ActMap map, ulong seed, bool clearDrawings)
    {
        try
        {
            _fields ??= GetInstanceFields(typeof(NMapScreen));
            foreach (var field in _fields)
                CleanField(__instance, field);
        }
        catch (Exception e)
        {
            GD.PushWarning($"[Ruina2] MapScreenStaleNodePatch failed: {e}");
        }
    }

    private static void CleanField(NMapScreen screen, FieldInfo field)
    {
        var value = field.GetValue(screen);
        switch (value)
        {
            case null:
                return;

            case GodotObject obj when !GodotObject.IsInstanceValid(obj):
                field.SetValue(screen, null);
                MainFile.Logger.Info("freed node reference cleared: " + field.Name);
                return;

            case IDictionary dict:
                var deadKeys = new List<object>();
                foreach (DictionaryEntry entry in dict)
                {
                    if (IsDead(entry.Key) || IsDead(entry.Value))
                        deadKeys.Add(entry.Key);
                }
                foreach (var key in deadKeys)
                    dict.Remove(key);
                if (deadKeys.Count > 0)
                    MainFile.Logger.Info("freed " + deadKeys.Count + " keys");
                return;

            case IList list when !list.IsFixedSize:
                var removed = 0;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (IsDead(list[i]))
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }
                if (removed > 0)
                    MainFile.Logger.Info("freed " + removed + " nodes");
                return;

            case IList array:
                var nulled = 0;
                for (var i = 0; i < array.Count; i++)
                {
                    if (IsDead(array[i]))
                    {
                        array[i] = null;
                        nulled++;
                    }
                }
                if (nulled > 0)
                    MainFile.Logger.Info("freed " + nulled + " slots");
                return;
        }
    }
    
    private static bool IsDead(object? o) =>
        o is GodotObject obj && !GodotObject.IsInstanceValid(obj);

    private static FieldInfo[] GetInstanceFields(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var result = new List<FieldInfo>();
        for (var t = type; t != null && t.Namespace?.StartsWith("Godot") != true; t = t.BaseType)
            result.AddRange(t.GetFields(flags));
        return result.ToArray();
    }
}