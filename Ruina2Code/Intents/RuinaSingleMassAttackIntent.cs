using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace Ruina2.Ruina2Code.Intents;

public class RuinaSingleMassAttackIntent : RuinaMassAttackIntent
{
    public override int Repeats => 1;

    protected override LocString IntentLabelFormat => new LocString("intents", "FORMAT_DAMAGE_SINGLE");

    public RuinaSingleMassAttackIntent(int damage)
    {
        DamageCalc = (Func<Decimal>) (() => damage);
    }

    public RuinaSingleMassAttackIntent(Func<Decimal> damageCalc) => DamageCalc = damageCalc;

    public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
    {
        return GetTargetedSingleDamage(owner);
    }

    public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
    {
        LocString intentLabelFormat = IntentLabelFormat;
        float totalDamage = GetTotalDamage(targets, owner);
        intentLabelFormat.Add("Damage", (int) totalDamage);
        return intentLabelFormat;
    }
}