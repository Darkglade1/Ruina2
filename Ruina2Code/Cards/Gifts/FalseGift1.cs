using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.Oz;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace Ruina2.Ruina2Code.Cards.Gifts;

[Pool(typeof(StatusCardPool))]
public class FalseGift1() : Ruina2Card(-1, CardType.Status,
    CardRarity.Status, TargetType.None), Oz.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new CardsVar(1), new("Status", 3)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        EnergyHoverTip,
        HoverTipFactory.FromCard<Void>()
    ];
    
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;
    
    public override string CustomPortraitPath => "false_gift.png".BigCardImagePath();
    
    public override string PortraitPath => "false_gift.png".CardImagePath();
    
    public async Task OnChosen()
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
        await CardPileCmd.AddToCombatAndPreview<Void>(Owner.Creature, PileType.Discard, DynamicVars["Status"].IntValue, null);
    }
}