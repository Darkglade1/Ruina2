using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(StatusCardPool))]
public class TestCard() : Ruina2Card(0, CardType.Status,
    CardRarity.Status, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CardCmd.Afflict<TestAffliction>(this, 1);
        await CommonActions.Draw(this, choiceContext);
    }
    
    public override int MaxUpgradeLevel => 0;
}