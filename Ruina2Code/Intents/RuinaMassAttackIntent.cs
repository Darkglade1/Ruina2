using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Intents;

public abstract class RuinaMassAttackIntent : RuinaAttackIntent, ICustomModel
{
    [CustomEnum] 
    public static IntentType MassAttack;
    
    protected override string IntentPrefix => "ATTACK";
    
    protected override string SpritePath => "intent_mass_attack.png".UIImagePath();

    public override IEnumerable<string> AssetPaths => ["intent_mass_attack.png".UIImagePath()];
    
    public override Texture2D GetTexture(IEnumerable<Creature> targets, Creature owner)
    {
        return GD.Load<Texture2D>("intent_mass_attack.png".UIImagePath());
    }
    public override string GetAnimation(IEnumerable<Creature> targets, Creature owner)
    {
        return _cachedAnimationName ?? (_cachedAnimationName = "mass_attack");
    }
    protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
    {
        LocString intentDescription;
        if (owner.Monster is AbstractAllyMonster ally && ally.IsAlly)
        {
            if (ally.MassAttackHitsPlayer)
            {
                intentDescription = new LocString("intents", "RUINA2-ALLY_FRIENDLY_FIRE_MASS_ATTACK.description");
            }
            else
            {
                intentDescription = new LocString("intents", "RUINA2-ALLY_MASS_ATTACK.description");
            }
        } else if (owner.Monster is AbstractMultiIntentMonster)
        {
            intentDescription = new LocString("intents", "RUINA2-MULTI_INTENT_MASS_ATTACK.description");
        }
        else 
        {
            intentDescription = base.GetIntentDescription(targets, owner);
        }
        intentDescription.Add("Damage", GetTargetedSingleDamage(owner));
        intentDescription.Add("Repeat", Repeats);
        return intentDescription;
    }
}