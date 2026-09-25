using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Elena;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class ArbitersJudgement() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null && Owner.Monster is Binah binah)
        {
            if (cardPlay.Target.Monster is Elena || cardPlay.Target.Monster is Vermilion)
            {
                binah.OtherSideTargetMonster = cardPlay.Target;
                binah.Targets[0] = cardPlay.Target;
                NCreature? creatureNode = binah.Creature.GetCreatureNode();
                if (creatureNode == null || !CombatState.IsLiveCombat())
                    return;
                TaskHelper.RunSafely(creatureNode.RefreshIntents());
            }
        }
    }
}