using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Intents;

public class RuinaDefendIntent : DefendIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString intentDescription;
        if (owner.Monster is AbstractAllyMonster ally && ally.IsAlly)
        {
            intentDescription = new LocString("intents", "RUINA2-ALLY_DEFEND.description");
        }
        else
        {
            intentDescription = base.GetIntentDescription(targets, owner);
        }
        return intentDescription;
    }
}