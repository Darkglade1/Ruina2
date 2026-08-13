using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.Act3.Twilight;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class FadingTwilight() : Ruina2Power
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
        if (creature.Monster is BigEgg || creature.Monster is LongEgg || creature.Monster is SmallEgg)
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, Owner, (int)(Owner.CurrentHp * ((float)Amount / 100)),
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, null, null);
        }
    }
}