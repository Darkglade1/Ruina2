using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Events.Act2;

public class ThePianist() : CustomEventModel()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(Retain, HoverTipFactory.FromCardWithCardHoverTips<Writhe>()),
        Option(Succumb)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("Curse", ModelDb.Card<Writhe>().Title),
        new StringVar("Enchantment", ModelDb.Enchantment<PerfectFit>().Title.GetFormattedText()),
    ];
    
    public async Task Retain()
    {
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(Owner!, RelicRarity.Rare).ToMutable(), Owner!);
        await CardPileCmd.AddCurseToDeck<Writhe>(Owner!);
        SetEventFinished(PageDescription("RETAIN"));
    }

    public async Task Succumb()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(Owner!, ModelDb.Enchantment<PerfectFit>(), 1, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Enchant<PerfectFit>(card, 1M);
            NCardEnchantVfx? child = NCardEnchantVfx.Create(card);
            if (child != null)
            {
                NRun? instance = NRun.Instance;
                if (instance != null)
                    instance.GlobalUi.CardPreviewContainer.AddChildSafely(child);
            }
        }
        SetEventFinished(PageDescription("SUCCUMB"));
    }

    public override string CustomInitialPortraitPath => "the_pianist.png".EventImagePath();
    public override string CustomBackgroundScenePath => SceneHelper.GetScenePath("events/background_scenes/" + ModelDb.Event<ThisOrThat>().Id.Entry.ToLowerInvariant());
}