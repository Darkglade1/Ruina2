using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractAllyMonster : AbstractMultiIntentMonster
{
    public bool IsAlly = true;
    public bool IsTargetableByPlayers = false;

    public AbstractAllyMonster()
    {
        ShouldClearBlockAtStartOfOwnTurn = false;
    }
    
    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        SetToSide(CombatSide.Player);
        FlipHorizontal();
    }
    
    protected void SetToSide(CombatSide side)
    {
        FieldInfo? backingField = typeof(Creature).GetField("<Side>k__BackingField", 
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField != null)
        {
            backingField.SetValue(Creature, side); 
        }
    }
    
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && IsAlly)
        {
            Creature.Block = 0;
        }
        return Task.CompletedTask;
    }

    public override bool ShouldAllowHitting(Creature creature)
    {
        if (creature.Monster == this)
        {
            if (IsAlly && IsTargetableByPlayers)
            {
                return true;
            }
            else
            {
                return creature.CombatState?.CurrentSide == CombatSide.Enemy;
            }
        }
        return true;
    }
}