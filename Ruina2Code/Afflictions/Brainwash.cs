using System.Reflection;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

namespace Ruina2.Ruina2Code.Afflictions;

public class Brainwash : Ruina2Affliction
{
    public override bool HasExtraCardText => true;
    protected override string? CustomOverlayPath => "apostle.tscn".AfflictionImagePath();
    
    [CustomEnum] public static TargetType AnyRuinaAlly;
    
    public override void AfterApplied()
    {
        FieldInfo? backingField = typeof(CardModel).GetField("<TargetType>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Card, AnyRuinaAlly); 
        }
    }

    public override void BeforeRemoved()
    {
        FieldInfo? backingField = typeof(CardModel).GetField("<TargetType>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Card, TargetType.AnyEnemy); 
        }
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card == Card && cardPlay.Card.Affliction is Brainwash && cardPlay.Target != null && cardPlay.Target.Monster is Tiph tiph)
        {
            tiph.IsTargetableByPlayers = true;
            tiph.IsTargetableByPlayersMutable = false;
        }
        return Task.CompletedTask;
    }
    
    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == Card && cardPlay.Card.Affliction is Brainwash && cardPlay.Target != null && cardPlay.Target.Monster is Tiph tiph)
        {
            tiph.IsTargetableByPlayers = false;
            tiph.IsTargetableByPlayersMutable = true;
        }
        return Task.CompletedTask;
    }
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card == Card)
        {
            modifiedCost = 0M;
            return true; 
        }
        modifiedCost = originalCost;
        return false;
    }
}

