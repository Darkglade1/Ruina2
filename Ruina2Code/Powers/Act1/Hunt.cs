using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Ruina2.Ruina2Code.Monsters.Act1.SpiderBud;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Hunt() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature.Monster is Spiderling)
        {
            if (Owner.Monster is SpiderBud spider)
            {
                Flash();
                spider.SpiderlingJustDied = true;
            }
        }
    }
}