using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Events.Act2;

public class Language() : Ruina2Event()
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Option(CrossSwords, HoverTipFactory.FromEnchantment<Sharp>()),
        Option(TrainEgo)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("EnchantmentAmt", 3),
        new StringVar("Enchantment", ModelDb.Enchantment<Sharp>().Title.GetFormattedText()),
    ];
    
    public async Task CrossSwords()
    {
        CardModel? card = (await CardSelectCmd.FromDeckForEnchantment(Owner!, ModelDb.Enchantment<Sharp>(), DynamicVars["EnchantmentAmt"].IntValue, ((Func<CardModel, bool>) (c => c.Type == CardType.Attack))!, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
        if (card != null)
        {
            CardCmd.Enchant<Sharp>(card, DynamicVars["EnchantmentAmt"].IntValue);
            NCardEnchantVfx? child = NCardEnchantVfx.Create(card);
            if (child != null)
            {
                NRun? instance = NRun.Instance;
                if (instance != null)
                    instance.GlobalUi.CardPreviewContainer.AddChildSafely(child);
            }
        }
        SetEventFinished(PageDescription("CROSS_SWORDS"));
    }

    public async Task TrainEgo()
    {
        var egoCards = EGOCardPool.GetAct2EgoCards();
        egoCards.StableShuffle(Owner!.PlayerRng.Rewards);
        var card = Owner.RunState.CreateCard(egoCards[0], Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(PageDescription("TRAIN_EGO"));
    }
}