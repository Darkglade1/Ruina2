using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class SanguineDesire() : EGOCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(18, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BleedEnemy>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var attackCommand = await CommonActions.CardAttack(this, play).Execute(choiceContext);
        int totalDamage = 0;
        foreach (var result in attackCommand.Results.SelectMany(r => r))
        {
            totalDamage += result.UnblockedDamage + result.OverkillDamage;
        }
        if (totalDamage > 0 && play.Target != null)
        {
            await PowerCmd.Apply<BleedEnemy>(choiceContext, play.Target, totalDamage / 2M, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}