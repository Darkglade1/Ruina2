using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class SmallBeak() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CostIncrease", 1), new("CardsAffected", 0)];
    
    public override Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player.Creature == Target)
        {
            if (player.PlayerCombatState != null)
            {
                foreach (CardModel card in player.PlayerCombatState.AllCards.Where(c => c.Affliction is Punished))
                {
                    CardCmd.ClearAffliction(card);
                }
            }
            DynamicVars["CardsAffected"].BaseValue = 0;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card.Owner.Creature == Target)
        {
            if (DynamicVars["CardsAffected"].BaseValue < Amount)
            {
                Flash();
                CardCmd.Afflict<Punished>(card, DynamicVars["CostIncrease"].BaseValue);
                DynamicVars["CardsAffected"].BaseValue++;
            }
        }
        return Task.CompletedTask;
    }
    
    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card.Affliction is Punished)
        {
            modifiedCost = originalCost + card.Affliction.Amount;
            return true;
        }
        modifiedCost = originalCost;
        return false;
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Owner && Target?.Player != null)
        {
            if (Target.Player.PlayerCombatState != null)
            {
                foreach (CardModel card in Target.Player.PlayerCombatState.AllCards.Where(c => c.Affliction is Punished))
                {
                    CardCmd.ClearAffliction(card);
                }
            }
        }
    }
}