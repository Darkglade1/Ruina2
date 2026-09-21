using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class FlameShield() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Burn>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Turns",0)];
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult _,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner && props.IsPoweredAttack() && dealer != null && cardSource != null)
        {
            if (dealer.Player != null)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<Burn>(dealer, PileType.Draw, Amount, null, CardPilePosition.Random);
            } else if (dealer.PetOwner != null)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<Burn>(dealer.PetOwner.Creature, PileType.Draw, Amount, null, CardPilePosition.Random);
            }
        }
    }
}