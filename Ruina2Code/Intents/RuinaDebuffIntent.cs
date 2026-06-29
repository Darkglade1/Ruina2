using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Intents;

public class RuinaDebuffIntent : DebuffIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        Creature? targetCreature = RuinaAttackIntent.GetIntentTargetedCreature(this, owner);
        LocString intentDescription;
        if (targetCreature?.Monster is AbstractAllyMonster)
        {
            intentDescription = new LocString("intents", "RUINA2-MULTI_INTENT_DEBUFF.description");
            intentDescription.Add("Target", targetCreature.Name);
        }
        else if (owner.Monster is AbstractAllyMonster ally && ally.IsAlly && targetCreature != null)
        {
            intentDescription = new LocString("intents", "RUINA2-ALLY_DEBUFF.description");
            intentDescription.Add("Target", targetCreature.Name);
        }
        else
        {
            intentDescription = base.GetIntentDescription(targets, owner);
        }
        return intentDescription;
    }
}