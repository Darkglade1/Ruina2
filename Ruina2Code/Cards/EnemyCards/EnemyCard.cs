using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Cards.EnemyCards;

[Pool(typeof(EnemyCardPool))]
public abstract class EnemyCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    EGOCard(cost, type, rarity, target)
{
    public static readonly SpireField<CardModel, Creature> EnemyCardOwner = new(() => null);
    public void SetDamage(Decimal damage)
    {
        AssertMutable();
        DynamicVars.Damage.BaseValue = damage;
    }
    
    public void SetRepeat(Decimal repeat)
    {
        AssertMutable();
        DynamicVars.Repeat.BaseValue = repeat;
    }
    
    public void SetBlock(Decimal block)
    {
        AssertMutable();
        DynamicVars.Block.BaseValue = block;
    }
    
    public void SetStrength(Decimal strength)
    {
        AssertMutable();
        DynamicVars.Strength.BaseValue = strength;
    }
    
    public void SetWeak(Decimal weak)
    {
        AssertMutable();
        DynamicVars.Weak.BaseValue = weak;
    }
    
    public void SetFrail(Decimal frail)
    {
        AssertMutable();
        DynamicVars["FrailPower"].BaseValue = frail;
    }
    
    public void SetVulnerable(Decimal vulnerable)
    {
        AssertMutable();
        DynamicVars.Vulnerable.BaseValue = vulnerable;
    }
}