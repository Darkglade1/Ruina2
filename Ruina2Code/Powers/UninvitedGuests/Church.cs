using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Eileen;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Church() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature.Monster is GearWorshipper)
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, null, null);
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            if (Owner.Monster is Eileen eileen)
            {
                if (eileen.minion1 == null || eileen.minion1.IsDead)
                {
                    Flash();
                    eileen.minion1 = await CreatureCmd.Add<GearWorshipper>(CombatState, "minion1");
                }
                if (eileen.minion2 == null || eileen.minion2.IsDead)
                {
                    Flash();
                    eileen.minion2 = await CreatureCmd.Add<GearWorshipper2>(CombatState, "minion2");
                }
            }
        }
    }
}