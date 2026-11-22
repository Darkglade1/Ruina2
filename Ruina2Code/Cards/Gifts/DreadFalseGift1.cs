using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.Oz;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace Ruina2.Ruina2Code.Cards.Gifts;

[Pool(typeof(StatusCardPool))]
public class DreadFalseGift1() : Ruina2Card(-1, CardType.Status,
    CardRarity.Status, TargetType.None), Oz.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new("Status", 2)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Void>()
    ];
    
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;
    
    public override string CustomPortraitPath => "false_gift.png".BigCardImagePath();
    
    public override string PortraitPath => "false_gift.png".CardImagePath();
    
    public async Task OnChosen()
    {
        await CardPileCmd.AddToCombatAndPreview<Void>(Owner.Creature, PileType.Draw, DynamicVars["Status"].IntValue, null, CardPilePosition.Random);
        await CardPileCmd.AddToCombatAndPreview<Void>(Owner.Creature, PileType.Discard, DynamicVars["Status"].IntValue, null);
    }
}