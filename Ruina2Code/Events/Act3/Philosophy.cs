using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
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

public class Philosophy() : Ruina2Event()
{
    private int heal;
    private CardModel chosenCard;
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<CardModel> list = Owner!.Deck.Cards.Where(c => (c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare) && c.IsRemovable).ToList();
        list.StableShuffle(Rng);
        chosenCard = list[0];
        if (chosenCard.Rarity == CardRarity.Rare)
        {
            heal = DynamicVars["HealRare"].IntValue;
        } else if (chosenCard.Rarity == CardRarity.Common)
        {
            heal = DynamicVars["HealCommon"].IntValue;
        }
        else
        {
            heal = DynamicVars["HealUncommon"].IntValue;
        }

        var acceptOption = Option(Accept, [HoverTipFactory.FromCard(chosenCard)]);
        acceptOption.Description.Add("Card", chosenCard.Title);
        acceptOption.Description.Add("Heal", heal);

        return [acceptOption, Option(Reject, HoverTipFactory.FromEnchantment<Pillar>(DynamicVars["EnchantAmt"].IntValue))];
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("EnchantAmt", 4),
        new ("HealCommon", 20),
        new ("HealUncommon", 35),
        new ("HealRare", 60),
        new StringVar("Enchantment", ModelDb.Enchantment<Pillar>().Title.GetFormattedText()),
    ];
    
    public override bool IsAllowed(IRunState runState)
    {
        return base.IsAllowed(runState) && runState.Players.All(p => p.Deck.Cards.Where(c => (c.Rarity == CardRarity.Common || c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare) && c.IsRemovable).ToList().Count > 0);
    }
    
    public async Task Accept()
    {
        await CardPileCmd.RemoveFromDeck(chosenCard);
        await CreatureCmd.Heal(Owner!.Creature, heal);
        SetEventFinished(PageDescription("ACCEPT"));
    }

    public async Task Reject()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(Owner!, ModelDb.Enchantment<Pillar>(), DynamicVars["EnchantAmt"].IntValue, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Enchant<Pillar>(card, DynamicVars["EnchantAmt"].IntValue);
            NCardEnchantVfx? child = NCardEnchantVfx.Create(card);
            if (child != null)
            {
                NRun? instance = NRun.Instance;
                if (instance != null)
                    instance.GlobalUi.CardPreviewContainer.AddChildSafely(child);
            }
        }
        SetEventFinished(PageDescription("REJECT"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Atziluth;
    }
}