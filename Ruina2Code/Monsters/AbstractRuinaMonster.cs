using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

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
}