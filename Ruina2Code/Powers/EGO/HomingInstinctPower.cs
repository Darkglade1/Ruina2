using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class HomingInstinctPower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<YellowBrickRoad>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            Creature? enemy = player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            if (enemy != null)
            {
                Flash();
                await PowerCmd.Apply<YellowBrickRoad>(new ThrowingPlayerChoiceContext(), enemy, Amount, Owner,  null);
            }
        }
    }
}