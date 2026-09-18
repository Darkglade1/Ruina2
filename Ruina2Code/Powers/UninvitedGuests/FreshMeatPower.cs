using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class FreshMeatPower : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private CardModel? _stolenCard;
    public CardModel? StolenCard
    {
        get => _stolenCard;
        set
        {
            AssertMutable();
            _stolenCard = value;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return StolenCard == null ? Array.Empty<IHoverTip>() : [HoverTipFactory.FromCard(StolenCard)];
        }
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (Owner == creature && StolenCard != null)
        {
            // Clear the removed flag
            var removedProp = typeof(CardModel).GetProperty("HasBeenRemovedFromState", BindingFlags.Public | BindingFlags.Instance);
            removedProp?.SetValue(StolenCard, false);

            // Re-register the card with combat state
            var allCardsField = typeof(CombatState).GetField("_allCards", BindingFlags.NonPublic | BindingFlags.Instance);
            var allCards = allCardsField?.GetValue(CombatState) as List<CardModel>;
            if (allCards != null && !allCards.Contains(StolenCard))
            {
                allCards.Add(StolenCard);
            }
            await CardPileCmd.Add(StolenCard, PileType.Hand);
        }
    }

    public async Task Steal(CardModel card)
    {
       Target = card.Owner.Creature; 
       StolenCard = card;
    }
    
}