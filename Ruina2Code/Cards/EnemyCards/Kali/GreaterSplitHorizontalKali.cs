using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Cards.EnemyCards.Kali;

public class GreaterSplitHorizontalKali() : EnemyCard(6, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    public override string CustomPortraitPath => "greater_split_horizontal.png".BigCardImagePath();
    public override string PortraitPath => "greater_split_horizontal.png".CardImagePath();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(40, ValueProp.Move), new PowerVar<Bleed>(0)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
    }
}