using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Actions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Greta;

namespace Ruina2.Ruina2Code.Monsters;

[GlobalClass]
public partial class NPurpleTearStanceButton : NAllyButton
{
    public int Stance;
    protected override void OnClick()
    {
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        if (localPlayer != null)
        {
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new PurpleTearStanceChangeAction(localPlayer, owner.Creature.ModelId, Stance));
        }
    }

    protected override IHoverTip? GetHoverTip()
    {
        string titleKey;
        string descriptionKey;
        if (Stance == Hod.SLASH)
        {
            titleKey = "RUINA2-SLASH_STANCE_BUTTON.title";
            descriptionKey = "RUINA2-SLASH_STANCE_BUTTON.description";
        } else if (Stance == Hod.PIERCE)
        {
            titleKey = "RUINA2-PIERCE_STANCE_BUTTON.title";
            descriptionKey = "RUINA2-PIERCE_STANCE_BUTTON.description";
        } else
        {
            titleKey = "RUINA2-GUARD_STANCE_BUTTON.title";
            descriptionKey = "RUINA2-GUARD_STANCE_BUTTON.description";
        }
        return new HoverTip(
            new LocString("static_hover_tips", titleKey),
            new LocString("static_hover_tips", descriptionKey));   
    }
}