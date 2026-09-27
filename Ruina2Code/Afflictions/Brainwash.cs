using System.Reflection;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Afflictions;

public class Brainwash : Ruina2Affliction
{
    public override bool HasExtraCardText => true;
    
    [CustomEnum] public static TargetType Self;
    
    public override void AfterApplied()
    {
        FieldInfo? backingField = typeof(CardModel).GetField("<TargetType>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Card, Self); 
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

