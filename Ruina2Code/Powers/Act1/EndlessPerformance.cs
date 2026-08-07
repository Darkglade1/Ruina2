using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.Performer;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class EndlessPerformance() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (CombatState.RoundNumber == 1)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<FirstChair>(CombatState.PlayerCreatures, PileType.Discard, 1, null);
            }
            if (CombatState.RoundNumber == 2)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<SecondChair>(CombatState.PlayerCreatures, PileType.Discard, 1, null);
            }
            if (CombatState.RoundNumber == 3)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<ThirdChair>(CombatState.PlayerCreatures, PileType.Discard, 1, null);
            }
            if (CombatState.RoundNumber == 4)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<FourthChair>(CombatState.PlayerCreatures, PileType.Discard, 1, null);
            }
        }
    }
    
    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card is PerformerCard)
        {
            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Discard, card.Owner);
        }
    }
}