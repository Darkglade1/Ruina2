using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;

namespace Ruina2.Ruina2Code.Events.Act2;

public class SocialSciences() : Ruina2Event()
{
    private int maxHPHeal;
    private PotionModel chosenPotion;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<PotionModel> list = Owner!.Potions.ToList();
        list.StableShuffle(Rng);
        chosenPotion = list[0];
        if (chosenPotion.Rarity == PotionRarity.Rare)
        {
            maxHPHeal = DynamicVars["MaxHPRare"].IntValue;
        } else if (chosenPotion.Rarity == PotionRarity.Common)
        {
            maxHPHeal = DynamicVars["MaxHPCommon"].IntValue;
        }
        else
        {
            maxHPHeal = DynamicVars["MaxHPUncommon"].IntValue;
        }

        var augmentOption = Option(Augment);
        augmentOption.Description.Add("Potion", chosenPotion.Title.GetFormattedText());
        augmentOption.Description.Add("MaxHP", maxHPHeal);

        return [Option(Drink), augmentOption];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(20),
        new ("MaxHPCommon", 8),
        new ("MaxHPUncommon", 12),
        new ("MaxHPRare", 16)
    ];
    
    public override bool IsAllowed(IRunState runState)
    {
        return base.IsAllowed(runState) && runState.Players.All(p => p.Potions.Count() >= 1);
    }
    
    public async Task Drink()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.IntValue);
        SetEventFinished(PageDescription("DRINK"));
    }

    public async Task Augment()
    {
        await PotionCmd.Discard(chosenPotion);
        await CreatureCmd.GainMaxHp(Owner!.Creature, maxHPHeal);
        SetEventFinished(PageDescription("DRINK"));
    }
    
    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        Owner!.CanUseOrRemovePotions = false;
        return Task.CompletedTask;
    }

    protected override void OnEventFinished() => Owner!.CanUseOrRemovePotions = true;
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}