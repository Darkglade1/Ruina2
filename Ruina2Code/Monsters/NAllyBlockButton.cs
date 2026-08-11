using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Actions;

namespace Ruina2.Ruina2Code.Monsters;

[GlobalClass]
public partial class NAllyBlockButton : NAllyButton
{
    public static int BLOCK_TRANSFER = 5;
    protected override void OnClick()
    {
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        if (localPlayer != null)
        {
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new AllyBlockButtonAction(localPlayer, owner.Creature.ModelId, owner.Creature.CombatId));
        }
    }

    protected override IHoverTip? GetHoverTip()
    {
        var hoverTip = new HoverTip(
            new LocString("static_hover_tips", "RUINA2-ALLY_BLOCK_BUTTON.title"),
            new LocString("static_hover_tips", "RUINA2-ALLY_BLOCK_BUTTON.description"),
            PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("atlases/intent_atlas.sprites/intent_defend.tres")));
        hoverTip.Description = String.Format(hoverTip.Description, BLOCK_TRANSFER);
        return hoverTip;
    }
}