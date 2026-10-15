using System.Reflection;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Afflictions;

public class Captivated : Ruina2Affliction
{
    public override bool HasExtraCardText => true;
    
    [CustomEnum] public static TargetType SpotlightOnly;
    
    public override void AfterApplied()
    {
        FieldInfo? backingField = typeof(CardModel).GetField("<TargetType>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Card, SpotlightOnly); 
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
}

