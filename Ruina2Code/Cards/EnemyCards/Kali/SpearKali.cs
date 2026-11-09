using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Cards.EnemyCards.Kali;

public class SpearKali() : EnemyCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    public override string CustomPortraitPath => "spear.png".BigCardImagePath();
    public override string PortraitPath => "spear.png".CardImagePath();
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0, ValueProp.Move), new RepeatVar(0)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
    }
}