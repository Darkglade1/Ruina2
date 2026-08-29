using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Ruina2.Ruina2Code.Afflictions;

public class Apostle : Ruina2Affliction
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Cards.Apostle>()];
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Card.Owner.Creature) && PileType.Hand.GetPile(Card.Owner).Cards.Contains(Card))
        {
            var result = await CardCmd.TransformTo<Cards.Apostle>(Card, CardPreviewStyle.MessyLayout);
            if (result != null && result.Value.cardAdded is Cards.Apostle apostle)
            {
                apostle.JustTransformed = true;
            }
            await Cmd.Wait(0.2f);
        }
    }
}