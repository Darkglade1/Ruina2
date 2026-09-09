using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Ruina2.Ruina2Code.Acts;
using Ruina2.Ruina2Code.Enchantments;

namespace Ruina2.Ruina2Code.Events.Act2;

public class Index() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Swipe, HoverTipFactory.FromCardWithCardHoverTips<Doubt>()),
        Option(Resist, HoverTipFactory.FromEnchantment<SteadyHeart>())
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Curse", ModelDb.Card<Doubt>().Title),
        new StringVar("Enchantment", ModelDb.Enchantment<SteadyHeart>().Title.GetFormattedText()),
    ];
    
    public async Task Swipe()
    {
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(Owner!).ToMutable(), Owner!);
        await CardPileCmd.AddCurseToDeck<Doubt>(Owner!);
        SetEventFinished(PageDescription("SWIPE"));
    }

    public async Task Resist()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(Owner!, ModelDb.Enchantment<SteadyHeart>(), 1, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Enchant<SteadyHeart>(card, 1M);
            NCardEnchantVfx? child = NCardEnchantVfx.Create(card);
            if (child != null)
            {
                NRun? instance = NRun.Instance;
                if (instance != null)
                    instance.GlobalUi.CardPreviewContainer.AddChildSafely(child);
            }
        }
        SetEventFinished(PageDescription("RESIST"));
    }
    
    protected override bool IsAllowedForAct(ActModel act)
    {
        return act is Briah;
    }
}