using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class BeastSeeking() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CostIncrease", 1), new("CardsAffected", 1)];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await CardPileCmd.AddToCombatAndPreview<Dazzled>(Owner, PileType.Draw, Amount, null, CardPilePosition.Random);
        }
    }
    
    public override Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player.Creature == Owner)
        {
            DynamicVars["CardsAffected"].BaseValue = 0;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card.Owner.Creature == Owner)
        {
            if (DynamicVars["CardsAffected"].BaseValue < Amount)
            {
                Flash();
                card.EnergyCost.AddThisCombat(DynamicVars["CostIncrease"].IntValue);
                DynamicVars["CardsAffected"].BaseValue++;
            }
        }
        return Task.CompletedTask;
    }
}