using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class GreenStem() : EGOCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PoisonPower>(6), new ExhaustiveVar(2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), play.Target, DynamicVars["PoisonPower"].IntValue , Owner.Creature, this);
        }
    }
    
    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card == this && CombatState != null)
        {
            foreach (var enemy in CombatState.HittableEnemies)
            {
                var poisonAmt = enemy.GetPowerAmount<PoisonPower>();
                if (poisonAmt > 0)
                {
                    await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), enemy, poisonAmt , Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Poison.UpgradeValueBy(2);
    }
}