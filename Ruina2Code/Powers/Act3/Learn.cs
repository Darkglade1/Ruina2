using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Learn() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Fragile",2), new("Block",4)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TempFragile>()];
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            Flash();
            await PowerCmd.Apply<TempFragile>(new ThrowingPlayerChoiceContext(), cardPlay.Card.Owner.Creature, DynamicVars["Fragile"].IntValue, Owner,  null);
        }
        if (cardPlay.Card.Type == CardType.Skill)
        {
            Flash();
            await PowerCmd.Apply<BlockNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner, DynamicVars["Block"].IntValue, Owner,  null);
        }   
    }
}