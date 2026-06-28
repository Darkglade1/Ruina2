using BaseLib.Abstracts;
using BaseLib.Audio;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Ruina2.Ruina2Code.Audio;

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
    
    protected CreatureAnimator GenerateAnimatorFromKeys(List<string> keys, MegaSprite controller)
    {
        var idle = new AnimState(keys[0], true);
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState(keys[0], idle);
        foreach (var key in keys)
        {
            if (key != keys[0])
            {
                var anim = new AnimState(key);
                animator.AddAnyState(key, anim);
            }
        }
        return animator;
    }

    protected async Task ApplyPowerAndSkipNextDurationTick<T>(IReadOnlyList<Creature> targets, int amount) where T : PowerModel
    {
        var powerList = await PowerCmd.Apply<T>(new ThrowingPlayerChoiceContext(), targets, amount, Creature, null);
        powerList[0].SkipNextDurationTick = true;
    }
    
    protected async Task AnimationAction(string animationKey, ModSound sfx, IReadOnlyList<Creature>? targets)
    {
        if (targets == null || targets[0].IsPlayer || targets[0].IsAlive)
        {
            await CreatureCmd.TriggerAnim(Creature, animationKey, 0);
            sfx.Play();
        }
    }

    protected async Task AnimationAction(string animationKey, ModSound sfx)
    {
        await AnimationAction(animationKey, sfx, null);
    }
}