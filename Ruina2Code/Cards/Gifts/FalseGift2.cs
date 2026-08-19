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
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Cards.Gifts;

[Pool(typeof(StatusCardPool))]
public class FalseGift2() : Ruina2Card(-1, CardType.Status,
    CardRarity.Status, TargetType.None), Oz.IChoosable
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2), new("TurnCount", 3), new("StrengthLoss", 5)];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];
    
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;
    
    public override string CustomPortraitPath => "false_gift.png".BigCardImagePath();
    
    public override string PortraitPath => "false_gift.png".CardImagePath();
    
    public async Task OnChosen()
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["StrengthPower"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["StrengthPower"].IntValue, Owner.Creature, this);
        (await PowerCmd.Apply<FalseGift2Power>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["TurnCount"].BaseValue, Owner.Creature, this))?.SetPowerLoss(DynamicVars["StrengthLoss"].BaseValue);
    }
}