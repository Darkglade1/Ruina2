using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class LifeFibers : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(4), new("HealIncrease", 4)];
    
    private bool _wasUsed;
    public override bool IsUsedUp => _wasUsed;
    
    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (!IsUsedUp)
                return;
            Status = RelicStatus.Disabled;
        }
    }
    
    [SavedProperty]
    public int HealAmount
    {
        get => DynamicVars.Heal.IntValue;
        set
        {
            AssertMutable();
            DynamicVars.Heal.BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }
    
    public override bool ShowCounter => !IsUsedUp;
    
    public override int DisplayAmount => DynamicVars.Heal.IntValue;


    public override bool ShouldDieLate(Creature creature)
    {
        return creature != Owner.Creature || WasUsed;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        WasUsed = true;
        await CreatureCmd.Heal(creature, Math.Max(1M, DynamicVars.Heal.BaseValue));
    }
    
    public override async Task AfterCombatVictory(CombatRoom _)
    {
        if (!IsUsedUp)
        {
            Flash();
            HealAmount += DynamicVars["HealIncrease"].IntValue;
        }
    }
}