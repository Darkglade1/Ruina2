using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Powers.EGO;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class MagicBullet() : EGOCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MagicBulletPower>(100), new PowerVar<StrengthPower>(2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded ? [HoverTipFactory.FromPower<StrengthPower>()] : [];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            var power = await PowerCmd.Apply<MagicBulletPower>(choiceContext, play.Target, 1, Owner.Creature, this);
            if (power != null)
            {
                power.DynamicVars["Increase"].BaseValue = DynamicVars["MagicBulletPower"].IntValue;
            }
            if (IsUpgraded)
            {
                await PowerCmd.Apply<MagicBulletTempStr>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].IntValue, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}