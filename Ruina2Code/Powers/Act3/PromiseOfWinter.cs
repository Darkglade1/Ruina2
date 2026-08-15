using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class PromiseOfWinter() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;
    public override PowerStackType StackType =>
        PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Frozen>();
    
    public override async Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            Flash();
            foreach (var player in CombatState.Players)
            {
                var cards = PileType.Discard.GetPile(player).Cards;
                for (int i = 0; i < Amount; i++)
                {
                    CardModel? card = player.RunState.Rng.CombatCardSelection.NextItem(cards);
                    if (card != null)
                    {
                        await CardCmd.Afflict<Frozen>(card, 1);
                        await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random);
                    }
                }
            }
        }
    }
}