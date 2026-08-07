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
using Ruina2.Ruina2Code.Vfx;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractRuinaMonster : CustomMonsterModel
{
    public bool IsMassAttacking;
    public StanceVfxController? StanceVfx;
    public static StanceVfxConfig WrathVfxConfig   => new(
    "res://Ruina2/images/vfx/wrath_aura.tscn"
    );

    protected bool LastMove(MonsterMoveStateMachine stateMachine, string moveId)
    {
        var log = stateMachine.StateLog;
        if (log.Count == 0) return false;
        return log[log.Count - 1].Id == moveId;
    }
    
    protected bool LastMoveBefore(MonsterMoveStateMachine stateMachine, string moveId)
    {
        var log = stateMachine.StateLog;
        if (log.Count < 2) return false;
        return log[log.Count - 2].Id == moveId;
    }
    
    protected bool LastMoveBeforeBefore(MonsterMoveStateMachine stateMachine, string moveId)
    {
        var log = stateMachine.StateLog;
        if (log.Count < 3) return false;
        return log[log.Count - 3].Id == moveId;
    }

    protected bool LastTwoMoves(MonsterMoveStateMachine stateMachine, string moveId)
    {
        var log = stateMachine.StateLog;
        if (log.Count < 2) return false;
        return log[log.Count - 1].Id == moveId && log[log.Count - 2].Id == moveId;
    }
    
    protected bool ThreeTurnCooldownHasPassedForMove(MonsterMoveStateMachine stateMachine, string moveId) {
        return stateMachine.StateLog.Count >= 3 && !LastMove(stateMachine, moveId) && !LastMoveBefore(stateMachine, moveId) && !LastMoveBeforeBefore(stateMachine, moveId);
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
        foreach (var power in powerList)
        {
            power.SkipNextDurationTick = true;
        }
    }
    
    protected async Task ApplyPowerAndSkipNextDurationTickIfNotPresent<T>(IReadOnlyList<Creature> targets, int amount) where T : PowerModel
    {
        foreach (var creature in targets)
        {
            var hadPower = creature.HasPower<T>();
            var power = await PowerCmd.Apply<T>(new ThrowingPlayerChoiceContext(), creature, amount, Creature, null);
            if (!hadPower && power != null)
            {
                power.SkipNextDurationTick = true;
            }
        }
    }
    
    protected void SetPosition(Vector2 position)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            node.Position = position;
        }
    }
    
    protected async Task AnimationAction(string animationKey, ModSound? sfx, IReadOnlyList<Creature>? targets, float volume)
    {
        if (targets == null || targets[0].IsPlayer || targets[0].IsAlive)
        {
            await CreatureCmd.TriggerAnim(Creature, animationKey, 0);
            if (sfx != null && Creature.IsAlive)
            {
                sfx.Play(0, volume);   
            }
        }
    }
    
    protected async Task AnimationAction(string animationKey, ModSound? sfx, IReadOnlyList<Creature>? targets)
    {
        await AnimationAction(animationKey, sfx, targets, 1);
    }

    protected async Task AnimationAction(string animationKey, ModSound? sfx)
    {
        await AnimationAction(animationKey, sfx, null, 1);
    }
    
    protected async Task AnimationAction(string animationKey, ModSound? sfx, float volume)
    {
        await AnimationAction(animationKey, sfx, null, volume);
    }
    
    protected async Task WaitAnimation()
    {
        await Cmd.Wait(0.5f);
    }
    
    protected async Task WaitAnimation(float waitTime)
    {
        await Cmd.Wait(waitTime);
    }
    
    protected virtual async Task ResetIdle()
    {
        await ResetIdle(0.5f);
    }
    
    protected virtual async Task ResetIdle(float waitTime)
    {
        IsMassAttacking = false;
        await WaitAnimation(waitTime);
        if (Creature.GetCreatureNode() != null)
        {
            await CreatureCmd.TriggerAnim(Creature, "Idle", 0);
        }
    }
}