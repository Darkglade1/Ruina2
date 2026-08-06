using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Meal() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (context.Creature == Owner)
        {
            var segment = new HealthBarForecastSegment(Amount, new Color(0.59f, 0.3f, 0), HealthBarForecastDirection.FromLeft);
            return [segment];
        }
        return [];
    }
}