using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Cards.EnemyCards.Kali;

public class FocusSpiritKali() : EnemyCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    public override string CustomPortraitPath => "focus_spirit.png".BigCardImagePath();
    public override string PortraitPath => "focus_spirit.png".CardImagePath();
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(0, ValueProp.Move), new PowerVar<StrengthPower>(0)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
    }
}