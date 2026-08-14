using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters.Act3.BlueStar;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class MeetAgain() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner)
        {
            if (Owner.Monster is Worshipper worshipper && Owner.CurrentHp <= Amount && worshipper.MeetState != null)
            {
                worshipper.SetMoveImmediate(worshipper.MeetState);
            }
        }
    }
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (context.Creature == Owner)
        {
            var segment = new HealthBarForecastSegment(Amount, Color.Color8(173, 216, 230), HealthBarForecastDirection.FromLeft);
            return [segment];
        }
        return [];
    }
}