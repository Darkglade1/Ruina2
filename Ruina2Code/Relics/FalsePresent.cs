using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class FalsePresent() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    private int numAffectedCards = 0;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(7),
        new CardsVar(2)
    ];
    
    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card.Owner == Owner)
        {
            if (numAffectedCards < DynamicVars.Cards.IntValue)
            {
                Flash();
                card.EnergyCost.SetUntilPlayed(0);
                numAffectedCards++;
            }
        }
        return Task.CompletedTask;
    }

    public override async Task AfterObtained()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature,  DynamicVars.HpLoss.IntValue, false);
    }
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        numAffectedCards = 0;
        return Task.CompletedTask;
    }
}