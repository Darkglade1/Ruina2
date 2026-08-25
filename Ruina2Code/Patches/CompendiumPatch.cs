using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using Ruina2.Ruina2Code.Cards.EGO;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary._Ready))]
public class EGOCardPoolCompendiumFilterPatch
{
    public static void Postfix(NCardLibrary __instance)
    {
        var egoCardPool = ModelDb.CardPool<EGOCardPool>();
        NCardPoolFilter filter = GenerateFilter(egoCardPool);
        filter.Loc = new LocString("card_library", "RUINA2-POOL_EGO_TIP");
        Node lastFilter = __instance._colorlessFilter;
        lastFilter.AddSibling(filter, forceReadableName: true);
        
        __instance._poolFilters.Add(filter, c => egoCardPool.AllCardIds.Contains(c.Id));
        //Connect signals
        Callable callable1 = Callable.From<NCardPoolFilter>(new Action<NCardPoolFilter>(__instance.UpdateCardPoolFilter));
        filter.Connect(NCardPoolFilter.SignalName.Toggled, callable1);
        filter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
        {
            __instance._lastHoveredControl = filter;
        }));
    }

    private static NCardPoolFilter GenerateFilter(CustomCardPoolModel pool)
    {
        //TextureRect named Image (56x56), position (4, 4), scale 0.9, pivot offset (28, 28)
        //  child TextureRect named Shadow (56x56), position (4, 3), scale 1.0, pivot offset (28, 28). Uses same image.
        //Control SelectionReticle (NSelectionReticle)
        //  Can probably just intantiate from scene scenes/ui/selection_reticle.tscn


        NCardPoolFilter filter = new()
        {
            Name = "FILTER-" + pool.Id,
            Size = new(64, 64),
            CustomMinimumSize = new(64, 64),
            FocusMode = Control.FocusModeEnum.All
        };

        //filter.Draw += filter.DrawDebug;

        Texture2D tex = PreloadManager.Cache.GetTexture2D("cards/icon.png".ImagePath());
        TextureRect image = new()
        {
            Name = "Image",
            Texture = tex,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new(56, 56),
            Position = new(4, 4),
            Scale = new(0.9f, 0.9f),
            PivotOffset = new(28, 28),
            Material = ShaderUtils.GenerateHsv(1, 1, 1)
        };

        TextureRect shadow = new()
        {
            Name = "Shadow", //not required
            Texture = tex,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new(56, 56),
            Position = new(4, 3),
            PivotOffset = new(28, 28),
            ShowBehindParent = true,
            Modulate = Colors.Black with { A = 0.25f },
        };

        image.AddChild(shadow);
        NSelectionReticle reticle = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("ui/selection_reticle"))
            .Instantiate<NSelectionReticle>();
        reticle.Name = "SelectionReticle";
        reticle.UniqueNameInOwner = true;

        filter.AddChild(image);
        image.Owner = filter;
        filter.AddChild(reticle);
        reticle.Owner = filter; //I think this is necessary for UniqueNameInOwner to work properly with GetNode

        return filter;
    }
}