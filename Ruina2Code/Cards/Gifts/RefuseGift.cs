using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Ruina2.Ruina2Code.Monsters.Act2.Oz;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Cards.Gifts;

[Pool(typeof(StatusCardPool))]
public class RefuseGift() : Ruina2Card(-1, CardType.Status,
    CardRarity.Status, TargetType.None), Oz.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Fragile>(2)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Fragile>()
    ];
    
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;
    
    public async Task OnChosen()
    {
        await PowerCmd.Apply<Fragile>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["Fragile"].IntValue, Owner.Creature, this);
    }
}