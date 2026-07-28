using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.Act2.redWolf;

public sealed class NightmareWolf : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 550, 500);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int FangDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int FangHits => 2;
    private int HuntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int HuntHits => 3;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int BlockAmount => 20;
    private int BleedAmount => 2;

    protected override string VisualsPath => "NightmareWolf/nightmare_wolf.tscn".MonsterImagePath();

    private const string CRUEL_CLAWS = "CRUEL_CLAWS";
    private const string FEROCIOUS_FANGS = "FEROCIOUS_FANGS";
    private const string BLOODSTAINED_HUNT  = "BLOODSTAINED_HUNT";
    private const string HOWL = "HOWL";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<LittleRed>();
        //await PowerCmd.Apply<SporeCloudPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }

    private MoveState GetClawState()
    {
        return new MoveState(CRUEL_CLAWS, Claws, new RuinaSingleAttackIntent(ClawDamage), new DefendIntent());
    }

    private MoveState GetFangState()
    {
        return new MoveState(FEROCIOUS_FANGS, Fangs, new RuinaMultiAttackIntent(FangDamage, FangHits), new RuinaDebuffIntent());
    }

    private MoveState GetHuntState()
    {
        return new MoveState(BLOODSTAINED_HUNT, Hunt, new RuinaMultiAttackIntent(HuntDamage, HuntHits));
    }

    private MoveState GetHowlState()
    {
        return new MoveState(HOWL, Howl, new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var clawState = GetClawState();
        var fangState = GetFangState();
        var huntState = GetHuntState();

        clawState.FollowUpState = fangState;
        huntState.FollowUpState = clawState;
        fangState.FollowUpState = huntState;

        states.Add(clawState);
        states.Add(huntState);
        states.Add(fangState);
        
        return new MonsterMoveStateMachine(states, fangState);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var howlState = GetHowlState();
        var fangState = GetFangState();
        var huntState = GetHuntState();

        fangState.FollowUpState = howlState;
        huntState.FollowUpState = fangState;
        howlState.FollowUpState = huntState;

        states.Add(howlState);
        states.Add(huntState);
        states.Add(fangState);
        
        return new MonsterMoveStateMachine(states, huntState);
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (intentNum == 0)
        {
            return CombatState.PlayerCreatures[0];
        }
        if (intentNum == 1 && OtherSideTargetMonster != null && OtherSideTargetMonster.IsAlive)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }

    private async Task Fangs(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < FangHits; i++)
        {
            await BiteAnimation(targets);
            await DamageCmd.Attack(FangDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
        await ApplyPowerAndSkipNextDurationTick<Bleed>(targets, BleedAmount);
    }
    
    private async Task Hunt(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < HuntHits; i++)
        {
            if (i % 2 == 0)
            {
                await ClawAnimation(targets);
            } else {
                await BiteAnimation(targets);
            }
            await DamageCmd.Attack(HuntDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Claws(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockAmount, ValueProp.Move, null);
        await ClawAnimation(targets);
        await DamageCmd.Attack(ClawDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }

    private async Task Howl(IReadOnlyList<Creature> targets)
    {
        await HowlAnimation();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
        await ResetIdle(1.0f);
    }

    public void OnRedDeath()
    {
        Sfx.WOLF_FOG.Play();
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is LittleRed red)
        {
            if (red.Creature.IsAlive && !red.killedWolf)
            {
                await red.Enrage();
            }
        }
    }

    private async Task BiteAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Bite", Sfx.WOLF_BITE, targets);
    }
    
    private async Task ClawAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Claw", Sfx.WOLF_SLASH, targets);
    }
    
    private async Task HowlAnimation()
    {
        await AnimationAction("Howl", Sfx.WOLF_HOWL);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Bite", "Claw", "Howl"], controller);
    }
}