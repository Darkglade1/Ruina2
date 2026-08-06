using BaseLib.Abstracts;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Afflictions;

public abstract class Ruina2Affliction : AfflictionModel, ICustomModel
{
    protected virtual string? CustomOverlayPath =>  $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.tscn".AfflictionImagePath();
    public virtual string? CustomLocalizationKey => Id.Entry;

    [HarmonyPatch(typeof (AfflictionModel), "OverlayPath", MethodType.Getter)]
    private static class IconPatch
    {
        private static bool Prefix(AfflictionModel __instance, ref string? __result)
        {
            if (!(__instance is Ruina2Affliction afflictionModel))
                return true;
            __result = afflictionModel.CustomOverlayPath;
            return __result == null;
        }
    }
    
    [HarmonyPatch(typeof(AfflictionModel), "get_Title")]
    [HarmonyPostfix]
    public static void TitlePostfix(
        AfflictionModel __instance,
        ref LocString __result)
    {
        if (__instance is not Ruina2Affliction customAffliction)
            return;

        if (string.IsNullOrWhiteSpace(customAffliction.CustomLocalizationKey))
            return;

        __result = new LocString(
            "afflictions",
            $"{customAffliction.CustomLocalizationKey}.title"
        );
    }

    [HarmonyPatch(typeof(AfflictionModel), "get_Description")]
    [HarmonyPostfix]
    public static void DescriptionPostfix(
        AfflictionModel __instance,
        ref LocString __result)
    {
        if (__instance is not Ruina2Affliction customAffliction)
            return;

        if (string.IsNullOrWhiteSpace(customAffliction.CustomLocalizationKey))
            return;

        __result = new LocString(
            "afflictions",
            $"{customAffliction.CustomLocalizationKey}.description"
        );
    }

    [HarmonyPatch(typeof(AfflictionModel), "get_ExtraCardText")]
    [HarmonyPostfix]
    public static void ExtraCardTextPostfix(
        AfflictionModel __instance,
        ref LocString __result)
    {
        if (__instance is not Ruina2Affliction customAffliction)
            return;

        if (string.IsNullOrWhiteSpace(customAffliction.CustomLocalizationKey))
            return;

        __result = new LocString(
            "afflictions",
            $"{customAffliction.CustomLocalizationKey}.extraCardText"
        );
    }
}