using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace Ruina2.Ruina2Code.Intents;

public class RuinaMultiAttackIntent : RuinaAttackIntent
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

  public RuinaMultiAttackIntent(int damage, int repeat)
  {
    DamageCalc = () => damage;
    _repeat = repeat;
  }

  public RuinaMultiAttackIntent(int damage, Func<int> repeatCalc)
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
