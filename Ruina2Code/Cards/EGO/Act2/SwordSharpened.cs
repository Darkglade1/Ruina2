using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using Ruina2.Ruina2Code.Powers.EGO;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class SwordSharpened() : EGOCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1), new("PlayThreshold", 3)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>(), 
        HoverTipFactory.FromPower<VulnerablePower>(),
        StunIntent.GetStaticHoverTip(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            int swordAmount = play.Target.GetPowerAmount<SwordSharpenedPower>();
            if (swordAmount >= DynamicVars["PlayThreshold"].IntValue - 1 && !play.IsAutoPlay && play.IsFirstInSeries)
            {
                await CreatureCmd.Stun(play.Target);
                await PowerCmd.Remove<SwordSharpenedPower>(play.Target);
            }
            else
            {
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), play.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), play.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
                if (!play.IsAutoPlay && play.IsFirstInSeries)
                {
                    await PowerCmd.Apply<SwordSharpenedPower>(new ThrowingPlayerChoiceContext(), play.Target,1, Owner.Creature, this);
                }
            }
        }
    }
    
    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation)
    {
        if (card == this && card.CurrentTarget != null)
        {
            int swordAmount = card.CurrentTarget.GetPowerAmount<SwordSharpenedPower>();
            if (swordAmount >= DynamicVars["PlayThreshold"].IntValue - 1)
            {
                return new CardLocation(card.Owner, PileType.Exhaust, CardPilePosition.Top);
            } else if (IsUpgraded)
            {
                return new CardLocation(card.Owner, PileType.Draw, CardPilePosition.Random);   
            }
        }
        return cardLocation;
    }

    protected override void OnUpgrade()
    {
    }
}