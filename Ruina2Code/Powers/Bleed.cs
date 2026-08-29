using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers;

public class Bleed() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer == Owner && props.IsPoweredAttack() && cardSource != null && cardSource.Type == CardType.Attack)
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner, null);
        }
    }
    
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.Player != null && Owner.Player.PlayerCombatState != null)
        {
            foreach (CardModel card in Owner.Player.PlayerCombatState.AllCards.Where(c => c.Type == CardType.Attack))
            {
                await CardCmd.Afflict<Afflictions.Bleed>(card, 1M);
            }
        }
    }
    
    public override Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner.Player != null && oldOwner.Player.PlayerCombatState != null)
        {
            foreach (CardModel card in oldOwner.Player.PlayerCombatState.AllCards.Where(c => c.Affliction is Afflictions.Bleed))
            {
                CardCmd.ClearAffliction(card);
            }
        }
        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner.Player != null && side == CombatSide.Player)
        {
            if (participants.Contains(Owner))
            {
                await PowerCmd.Remove(this);  
            }
        }
    }
}