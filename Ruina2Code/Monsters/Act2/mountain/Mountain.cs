using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;

namespace Ruina2.Ruina2Code.Monsters.Act2.mountain;

public sealed class Mountain : AbstractMultiIntentMonster
{
    private int Stage3HP;
    private int Stage2HP;
    private int Stage1HP;
    public static int STAGE3 = 3;
    public static int STAGE2 = 2;
    public static int STAGE1 = 1;
    private int phase = STAGE3;
    public override int MinInitialHp => Stage3HP;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => phase;
    private static float REVIVE_PERCENT = 0.50f;
    private static float STARTING_PERCENT = 0.50f;

    private int DevourDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 14);
    private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int RamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int WeakAmt => 1;
    private int FrailAmt => 1;
    private int ScreechStatus => 3;
    private int VomitStatus => 2;

    protected override string VisualsPath => "Mountain/mountain.tscn".MonsterImagePath();

    private const string DEVOUR = "DEVOUR";
    private const string BITE = "BITE";
    private const string SCREECH  = "SCREECH";
    private const string RAM = "RAM";
    private const string VOMIT = "VOMIT";
    private const string REVIVE = "REVIVE";
    private const string NONE = "NONE";

    public Mountain()
    {
        Stage3HP = AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 135, 125);
        Stage2HP = (int)Math.Round(Stage3HP * 0.8f);
        Stage1HP = (int)Math.Round(Stage3HP * 0.4f);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Sfx.SPAWN.Play();
        OtherSideTargetMonster = FindTarget<Corpse>();
        Creature.CurrentHp = (int)(Creature.MaxHp * STARTING_PERCENT);
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            node.Position = new Vector2(400, 200);
        }
        //await PowerCmd.Apply<SporeCloudPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }

    private MoveState GetDevourState()
    {
        return new MoveState(DEVOUR, Devour, new RuinaSingleAttackIntent(DevourDamage), new HealIntent());
    }

    private MoveState GetBiteState()
    {
        return new MoveState(BITE, Bite, new RuinaSingleAttackIntent(BiteDamage), new RuinaDebuffIntent());
    }

    private MoveState GetRamState()
    {
        return new MoveState(RAM, Ram, new RuinaSingleAttackIntent(RamDamage));
    }

    private MoveState GetScreechState()
    {
        return new MoveState(SCREECH, Screech, new DebuffIntent());
    }
    
    private MoveState GetVomitState()
    {
        return new MoveState(VOMIT, Vomit, new DebuffIntent(true));
    }
    
    private MoveState GetReviveState()
    {
        return new MoveState(REVIVE, Revive, new HealIntent());
    }
    
    private MoveState GetNoneState()
    {
        return new MoveState(NONE, _ => Task.CompletedTask);
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBiteState();
        var state2 = GetDevourState();
        var state3 = GetRamState();
        var state4 = GetReviveState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBiteState();
        var state2 = GetDevourState();
        var state3 = GetRamState();
        var state4 = GetScreechState();
        var state5 = GetNoneState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;
        state5.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private MonsterMoveStateMachine GenerateIntent3StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDevourState();
        var state2 = GetVomitState();
        var state3 = GetNoneState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 2);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (Creature.CurrentHp <= 0 && intentNum == 0)
        {
            return REVIVE;
        }  
        if (Creature.CurrentHp <= 0 && intentNum != 0)
        {
            return NONE;
        }  
        if (intentNum == 0)
        {
            if (phase == STAGE1)
            {
                return DEVOUR;
            } else if (phase == STAGE2)
            {
                List<string> possibilities = new List<string>();
                if (!LastMove(stateMachine, DEVOUR)) {
                    possibilities.Add(DEVOUR);
                }
                if (!LastMove(stateMachine, BITE)) {
                    possibilities.Add(BITE);
                }
                if (!LastMove(stateMachine, SCREECH) && !LastMoveBefore(stateMachine, SCREECH)) {
                    possibilities.Add(SCREECH);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            } else {
                List<string> possibilities = new List<string>();
                if (!LastTwoMoves(stateMachine, RAM)) {
                    possibilities.Add(RAM);
                }
                if (!LastTwoMoves(stateMachine, BITE)) {
                    possibilities.Add(BITE);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        } else if (intentNum == 1)
        {
            if (phase == STAGE2)
            {
                return DEVOUR;
            }
            else
            {
                List<string> possibilities = new List<string>();
                if (!LastTwoMoves(stateMachine, RAM)) {
                    possibilities.Add(RAM);
                }
                if (!LastTwoMoves(stateMachine, BITE)) {
                    possibilities.Add(BITE);
                }
                return possibilities[rng.NextInt(possibilities.Count)];
            }
        }
        else
        {
            if (LastMove(stateMachine, DEVOUR))
            {
                return VOMIT;
            }
            else
            {
                return DEVOUR;
            }
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine(), GenerateIntent3StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            if (phase == STAGE1 && OtherSideTargetMonster != null)
            {
                return OtherSideTargetMonster;
            }
            else
            {
                return CombatState.PlayerCreatures[0];
            }
        }
        if (intentNum == 1)
        {
            if (phase == STAGE3)
            {
                return CombatState.PlayerCreatures[0];
            } else if (phase == STAGE2 && NextMoves[intentNum].Id == DEVOUR && OtherSideTargetMonster != null)
            {
                return OtherSideTargetMonster;
            }
            else
            {
                return CombatState.PlayerCreatures[0];
            }
        }
        if (intentNum == 2)
        {
            if (phase == STAGE3 && NextMoves[intentNum].Id == DEVOUR && OtherSideTargetMonster != null)
            {
                return OtherSideTargetMonster;
            }
            else
            {
                return CombatState.PlayerCreatures[0];
            }
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Devour(IReadOnlyList<Creature> targets)
    {
        await BiteAnimation(targets);
        var attackCommand = await DamageCmd.Attack(DevourDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await CreatureCmd.Heal(Creature,
            attackCommand.Results.SelectMany(r => r)
                .Sum((Func<DamageResult, int>)(r => r.TotalDamage + r.OverkillDamage)));
        await ResetIdle(1.0f);
    }
    
    private async Task Bite(IReadOnlyList<Creature> targets)
    {
        await BiteAnimation(targets);
        await DamageCmd.Attack(BiteDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, WeakAmt, Creature,  null);
        await ResetIdle(0.75f);
    }
    
    private async Task Ram(IReadOnlyList<Creature> targets)
    {
        await RamAnimation(targets);
        await DamageCmd.Attack(RamDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle(0.75f);
    }

    private async Task Screech(IReadOnlyList<Creature> targets)
    {
        await ScreechAnimation();
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, ScreechStatus,null, CardPilePosition.Random);
        await ResetIdle(1.0f);
    }
    
    private async Task Vomit(IReadOnlyList<Creature> targets)
    {
        await VomitAnimation();
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmt, Creature,  null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, VomitStatus,null, CardPilePosition.Random);
        await ResetIdle(1.0f);
    }
    
    private async Task Revive(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Heal(Creature, (int)(Creature.MaxHp * REVIVE_PERCENT));
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        
    }

    private async Task BiteAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Bite" + phase, Sfx.WOLF_BITE, targets);
    }
    
    private async Task RamAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ram", Sfx.RAM, targets);
    }
    
    private async Task ScreechAnimation()
    {
        await AnimationAction("Screech", Sfx.SCREECH, 0.3f);
    }
    
    private async Task VomitAnimation()
    {
        await AnimationAction("Vomit", Sfx.VOMIT, 0.5f);
    }
    
    protected override async Task ResetIdle()
    {
        await WaitAnimation();
        await CreatureCmd.TriggerAnim(Creature, "Idle" + phase, 0);
    }
    
    protected override async Task ResetIdle(float waitTime)
    {
        await WaitAnimation(waitTime);
        await CreatureCmd.TriggerAnim(Creature, "Idle" + phase, 0);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Idle1", "Idle2", "Idle3", "Bite1", "Bite2", "Bite3", "Ram", "Screech", "Vomit"], controller);
    }
}