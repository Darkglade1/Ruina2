using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

[Pool(typeof(TokenCardPool))]
public class FierceLook() : Ruina2Card(-1, CardType.Skill,
    CardRarity.Token, TargetType.Self), TodaysExpression.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override bool CanBeGeneratedInCombat => false;
    
    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].IntValue, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}