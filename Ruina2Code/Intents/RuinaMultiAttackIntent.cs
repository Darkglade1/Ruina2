using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace Ruina2.Ruina2Code.Intents;

public class RuinaMultiAttackIntent : RuinaAttackIntent
{
  public readonly int _repeat;
  public readonly Func<int>? _repeatCalc;

  protected override LocString IntentLabelFormat => new LocString("intents", "FORMAT_DAMAGE_MULTI");

  public override int Repeats
  {
    get
    {
      Func<int> repeatCalc = this._repeatCalc;
      return repeatCalc == null ? this._repeat : repeatCalc();
    }
  }

  public RuinaMultiAttackIntent(int damage, int repeat)
  {
    this.DamageCalc = (Func<Decimal>) (() => (Decimal) damage);
    this._repeat = repeat;
  }

  public RuinaMultiAttackIntent(int damage, Func<int> repeatCalc)
  {
    this.DamageCalc = (Func<Decimal>) (() => (Decimal) damage);
    this._repeatCalc = repeatCalc;
  }

  public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
  {
    return this.GetTargetedSingleDamage(owner) * this.Repeats;
  }

  public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
  {
    LocString intentLabelFormat = this.IntentLabelFormat;
    float singleDamage = (float) this.GetTargetedSingleDamage(owner);
    intentLabelFormat.Add("Damage", (Decimal) (int) singleDamage);
    intentLabelFormat.Add("Repeat", (Decimal) this.Repeats);
    return intentLabelFormat;
  }
}
