using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Enchantments;

namespace Ruina2.Ruina2Code.Events.Act3;

public class Thumb() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Option(Supplies), Option(Raid, HoverTipFactory.FromEnchantment<Vanishing>())];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("NumCards", 20),
        new CardsVar(1),
        new StringVar("Enchantment", ModelDb.Enchantment<Vanishing>().Title.GetFormattedText()),
    ];
    
    public async Task Supplies()
    {
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds([Owner!.Character.CardPool], (Func<CardModel, bool>) (c => c.Rarity == CardRarity.Uncommon)).WithFlags(CardCreationFlags.NoRarityModification);
        List<CardCreationResult> list = CardFactory.CreateForReward(Owner!, DynamicVars["NumCards"].IntValue, options).ToList();
        CardSelectorPrefs prefs = new CardSelectorPrefs(L10NLookup("RUINA2-THUMB.pages.SUPPLIES.selectionScreenPrompt"), DynamicVars.Cards.IntValue);
        await SelectCardsToAddToDeckFromGrid(list, prefs);
        SetEventFinished(PageDescription("SUPPLIES"));
    }

    public async Task Raid()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(Owner!, ModelDb.Enchantment<Vanishing>(), 1, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Enchant<Vanishing>(card, 1);
            NCardEnchantVfx? child = NCardEnchantVfx.Create(card);
            if (child != null)
            {
                NRun? instance = NRun.Instance;
                if (instance != null)
                    instance.GlobalUi.CardPreviewContainer.AddChildSafely(child);
            }
        }
        SetEventFinished(PageDescription("RAID"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}