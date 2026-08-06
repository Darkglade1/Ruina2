using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Ruina2.Ruina2Code.Monsters.Act1.FairyFestival;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class Satiation() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new("StrengthLoss",1)];
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && Owner.Monster is FairyQueen queen)
        {
            List<Creature> edibleMinions = new List<Creature>();
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is FairyMass && enemy.HasPower<Meal>())
                {
                    var mealPower = enemy.GetPower<Meal>();
                    if (mealPower != null)
                    {
                        if (enemy.CurrentHp <= mealPower.Amount)
                        {
                            edibleMinions.Add(enemy);
                        }
                    }
                }
            }

            if (edibleMinions.Count > 0)
            {
                await queen.ConsumeMinions(edibleMinions, Amount, DynamicVars["StrengthLoss"].IntValue);
            }
        }
    }
}