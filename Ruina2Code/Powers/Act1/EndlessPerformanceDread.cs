using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.Performer;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Hooks;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class EndlessPerformanceDread() : Ruina2Power, IAfterTransform
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override string CustomPackedIconPath => "endless_performance.png".PowerImagePath();
    public override string CustomBigIconPath => "endless_performance.png".BigPowerImagePath();

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player.PlayerCombatState != null && player.PlayerCombatState.TurnNumber == 1)
        {
            Flash();
            await CardPileCmd.AddToCombatAndPreview<FirstChair>(CombatState.PlayerCreatures, PileType.Draw, 1, null, CardPilePosition.Random);
            await CardPileCmd.AddToCombatAndPreview<SecondChair>(CombatState.PlayerCreatures, PileType.Draw, 1, null, CardPilePosition.Random);
            await CardPileCmd.AddToCombatAndPreview<ThirdChair>(CombatState.PlayerCreatures, PileType.Draw, 1, null, CardPilePosition.Random);
            await CardPileCmd.AddToCombatAndPreview<FourthChair>(CombatState.PlayerCreatures, PileType.Draw, 1, null, CardPilePosition.Random);
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
            var result = await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Discard, card.Owner);
            CardCmd.PreviewCardPileAdd(result);
            await Cmd.Wait(1f);
        }
    }

    public async Task AfterTransform(IEnumerable<CardTransformation> transformations)
    {
        foreach (var transformation in transformations)
        {
            if (transformation.Original is PerformerCard)
            {
                Flash();
                var result = await CardPileCmd.AddGeneratedCardToCombat(transformation.Original.CreateClone(), PileType.Discard, transformation.Original.Owner);
                CardCmd.PreviewCardPileAdd(result);
                await Cmd.Wait(1f);
            }
        }
    }
}