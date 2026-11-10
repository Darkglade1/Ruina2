using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class ScorchingWound : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    private int _combatsLeft = 3;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new ("Combats", 3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Burn>();
    
    public override bool IsUsedUp => CombatsLeft <= 0;

    public override bool ShowCounter => false;
    
    [SavedProperty]
    public int CombatsLeft
    {
        get => _combatsLeft;
        set
        {
            AssertMutable();
            _combatsLeft = value;
            DynamicVars["Combats"].BaseValue = _combatsLeft;
            InvokeDisplayAmountChanged();
            if (!IsUsedUp)
                return;
            Status = RelicStatus.Disabled;
        }
    }

    public override async Task BeforeCombatStart()
    {
        if (CombatsLeft <= 0)
            return;
        await CardPileCmd.AddToCombatAndPreview<Burn>(Owner.Creature, PileType.Draw, DynamicVars.Cards.IntValue, Owner, CardPilePosition.Random);
        CombatsLeft--;
        Flash();
    }
}