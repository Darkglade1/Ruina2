using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class BlueReverb() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;
    public override PowerStackType StackType =>
        PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<Vibration>();
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            Flash();
            await PowerCmd.Apply<Vibration>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, Amount, Owner,  null);
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is AbstractAllyMonster)
                {
                    await PowerCmd.Apply<Vibration>(new ThrowingPlayerChoiceContext(), enemy, Amount, Owner,  null);
                }
            }
        }
    }
}