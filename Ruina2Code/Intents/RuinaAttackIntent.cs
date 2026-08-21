using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Monsters.Act3.BigBird;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Intents;

public abstract class RuinaAttackIntent : AttackIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        Creature? targetCreature = GetIntentTargetedCreature(this, owner);
        LocString intentDescription;
        if (owner.Monster is AbstractAllyMonster ally && ally.IsAlly && targetCreature != null)
        {
            intentDescription = new LocString("intents", "RUINA2-ALLY_ATTACK.description");
            intentDescription.Add("Target", targetCreature.Name);
        } else if (targetCreature?.Monster is AbstractMultiIntentMonster)
        {
            intentDescription = new LocString("intents", "RUINA2-MULTI_INTENT_ATTACK.description");
            intentDescription.Add("Target", targetCreature.Name);
        }
        else
        {
            intentDescription = base.GetIntentDescription(targets, owner);
        }
        intentDescription.Add("Damage", GetTargetedSingleDamage(owner));
        intentDescription.Add("Repeat", Repeats);
        return intentDescription;
    }
    protected int GetTargetedSingleDamage(Creature owner)
    {
        Creature? targetCreature = GetIntentTargetedCreature(this, owner);
        Decimal totalDamage = 0;
        Player? player = LocalContext.GetMe(owner.CombatState);;
        if (targetCreature == null)
        { 
            targetCreature = player?.Creature;
        }

        if (player != null)
        {
            totalDamage = Hook.ModifyDamage(player.RunState, player.Creature.CombatState, targetCreature, owner, DamageCalc(), ValueProp.Move, null, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
            if (targetCreature != null && targetCreature.HasPower<Enchanted>() && owner.Monster is BigBird)
            {
                totalDamage = 999;
            }
        }
        return Math.Max(0, (int) totalDamage);
    }

    public static Creature? GetIntentTargetedCreature(AbstractIntent intent, Creature owner)
    {
        if (owner.Monster is AbstractMultiIntentMonster monster)
        {
            int intentIndex = 0;
            var nextMoves = monster.NextMoves;
            for (int i = 0; i < nextMoves.Count; i++)
            {
                if (nextMoves[i].Intents.Contains(intent))
                {
                    intentIndex = i;
                    break;
                }
            }
            return monster.Targets[intentIndex];
        }
        return null;
    }
}