using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Ruina2.Ruina2Code.Powers;

public class MonsterRegen() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.IsDead)
            return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }
    
    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
}