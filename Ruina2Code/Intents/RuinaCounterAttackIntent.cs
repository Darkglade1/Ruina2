using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Intents;

public abstract class RuinaCounterAttackIntent : RuinaAttackIntent, ICustomModel
{
    [CustomEnum] 
    public static IntentType CounterAttack;
    
    protected override string IntentPrefix => "ATTACK";
    
    protected override string SpritePath => "intent_counter_attack.png".UIImagePath();

    public override IEnumerable<string> AssetPaths => ["intent_counter_attack.png".UIImagePath()];
    
    public override Texture2D GetTexture(IEnumerable<Creature> targets, Creature owner)
    {
        return GD.Load<Texture2D>("intent_counter_attack.png".UIImagePath());
    }
    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner)
    {
        return _cachedAnimationName ?? (_cachedAnimationName = "counter_attack");
    }
    
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString intentDescription = new LocString("intents", "RUINA2-COUNTER_ATTACK.description");
        intentDescription.Add("Damage", GetTargetedSingleDamage(owner));
        intentDescription.Add("Repeat", Repeats);
        return intentDescription;
    }
}