using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace Ruina2.Ruina2Code.Intents;

public class RuinaMultiMassAttackIntent : RuinaMassAttackIntent
{
  public readonly int _repeat;
  public readonly Func<int>? _repeatCalc;

  protected override LocString IntentLabelFormat => new ("intents", "FORMAT_DAMAGE_MULTI");

  public override int Repeats
  {
    get
    {
      Func<int> repeatCalc = _repeatCalc;
      return repeatCalc == null ? _repeat : repeatCalc();
    }
  }
  
  public RuinaMultiMassAttackIntent(Func<decimal> damageCalc, int repeat)
  {
    DamageCalc = damageCalc;
    _repeat = repeat;
  }

  public RuinaMultiMassAttackIntent(int damage, int repeat)
  {
    DamageCalc = () => damage;
    _repeat = repeat;
  }

  public RuinaMultiMassAttackIntent(int damage, Func<int> repeatCalc)
  {
    DamageCalc = () => damage;
    _repeatCalc = repeatCalc;
  }

  public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
  {
    return GetTargetedSingleDamage(owner) * Repeats;
  }

  public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
  {
    LocString intentLabelFormat = IntentLabelFormat;
    float singleDamage = GetTargetedSingleDamage(owner);
    intentLabelFormat.Add("Damage", (int) singleDamage);
    intentLabelFormat.Add("Repeat", Repeats);
    return intentLabelFormat;
  }
}
