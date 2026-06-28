using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractRuinaMonster : CustomMonsterModel
{
    protected bool LastMove(MonsterMoveStateMachine stateMachine, string moveId)
    {
        var log = stateMachine.StateLog;
        if (log.Count == 0) return false;
        return log[log.Count - 1].Id == moveId;
    }

    protected bool LastTwoMoves(MonsterMoveStateMachine stateMachine, string moveId)
    {
        var log = stateMachine.StateLog;
        if (log.Count < 2) return false;
        return log[log.Count - 1].Id == moveId && log[log.Count - 2].Id == moveId;
    }

    protected void FlipHorizontal()
    {
        if (NCombatRoom.Instance != null)
        {
            var creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
            if (creatureNode != null)
            {
                creatureNode.Body.Scale *= new Vector2(-1f, 1f);
            }
        }
    }
}