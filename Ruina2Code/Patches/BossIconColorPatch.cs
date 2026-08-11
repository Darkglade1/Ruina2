using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(NBossMapPoint), "RefreshColorInstantly")]
public static class BossIconPatchRefreshColor
{
    public static void Postfix(NBossMapPoint __instance)
    {
        if (__instance._act is AbstractRuinaAct ruinaAct)
        {
            __instance._placeholderOutline.SelfModulate = ruinaAct.BossIconBgColor;
        }
    }
}