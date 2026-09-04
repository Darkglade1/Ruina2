using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class ParadiseLostPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars["CardCounter"].IntValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("CardCounter",0), new("ExhaustAmount",0)];
    
    public void SetExhaustAmount(Decimal exhaustAmount)
    {
        AssertMutable();
        DynamicVars["ExhaustAmount"].BaseValue = exhaustAmount;
    }
    
    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card.Owner.Creature == Owner)
        {
            DynamicVars["CardCounter"].BaseValue += 1;
            if (DynamicVars["CardCounter"].BaseValue >= 12)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
                DynamicVars["CardCounter"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
            return;
        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, player, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, DynamicVars["ExhaustAmount"].IntValue), null, this))
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
    }
}