using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

[Pool(typeof(TokenCardPool))]
public class ShyLook() : Ruina2Card(-1, CardType.Skill,
    CardRarity.Token, TargetType.Self), TodaysExpression.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15, ValueProp.Unpowered)];

    public override bool CanBeGeneratedInCombat => false;
    
    public async Task OnChosen(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}