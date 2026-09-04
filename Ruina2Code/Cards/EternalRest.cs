using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(TokenCardPool))]
public class EternalRest() : Ruina2Card(0,
    CardType.Attack, CardRarity.Token,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move), new RepeatVar(5)];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    private int sfxCounter = 0;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardAttack(this, play, DynamicVars.Repeat.IntValue).BeforeDamage(beforeDamage).Execute(choiceContext);
    }
    private Task beforeDamage()
    {
        if (sfxCounter % 2 == 0)
        {
            Sfx.FuneralAtkBlack.Play();
        }
        else
        {
            Sfx.FuneralAtkWhite.Play();
        }
        sfxCounter++;
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}