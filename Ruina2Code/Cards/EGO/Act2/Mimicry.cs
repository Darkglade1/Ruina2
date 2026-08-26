using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class Mimicry() : EGOCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(13, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        Sfx.NothingGoodbye.Play();
        var attackCommand = await CommonActions.CardAttack(this, play).Execute(choiceContext);
        int totalHeal = 0;
        foreach (var result in attackCommand.Results.SelectMany(r => r))
        {
            totalHeal += result.UnblockedDamage + result.OverkillDamage;
        }
        if (totalHeal > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, totalHeal);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}