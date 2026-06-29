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

namespace Ruina2.Ruina2Code.Intents;

public abstract class RuinaAttackIntent : AttackIntent
{
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString intentDescription = base.GetIntentDescription(targets, owner);
        intentDescription.Add("Damage", (Decimal) this.GetTargetedSingleDamage(owner));
        intentDescription.Add("Repeat", (Decimal) this.Repeats);
        return intentDescription;
    }
    protected int GetTargetedSingleDamage(Creature owner)
    {
        Creature? targetCreature = null;
        if (owner.Monster is AbstractMultiIntentMonster monster)
        {
            int intentIndex = 0;
            var nextMoves = monster.NextMoves;
            for (int i = 0; i < nextMoves.Count; i++)
            {
                if (nextMoves[i].Intents.Contains(this))
                {
                    intentIndex = i;
                    break;
                }
            }
            targetCreature = monster.Targets[intentIndex];
        }
        Decimal totalDamage = 0;
        Player? player = LocalContext.GetMe(owner.CombatState);;
        if (targetCreature == null)
        { 
            targetCreature = player?.Creature;
        }

        if (player != null)
        {
            totalDamage = Hook.ModifyDamage(player.RunState, player.Creature.CombatState, targetCreature, owner, DamageCalc(), ValueProp.Move, null, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);   
        }
        return Math.Max(0, (int) totalDamage);
    }
}