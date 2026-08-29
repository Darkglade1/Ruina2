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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act1;

namespace Ruina2.Ruina2Code.Monsters.Act1.BlackSwan;

public sealed class BlackSwan : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 180, 160);
    public override int MaxInitialHp => MinInitialHp;
    
    private int WritheDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int WritheHits => 2;
    private int RealityDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int ShriekDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    
    private int BlockAmt => 7;
    private int StrAmt => 1;
    private int StatusAmt => 2;
    private int ErosionAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    protected override string VisualsPath => "BlackSwan/black_swan.tscn".MonsterImagePath();

    private const string WRITHE = "WRITHE";
    private const string PARASOL = "PARASOL";
    private const string REALITY = "REALITY";
    public const string SHRIEK = "SHRIEK";

    public bool Enraged = false;
    public int NumActiveBrothers = 0;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Dream>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private MoveState GetWritheState()
    {
        return new MoveState(WRITHE, Writhe, new MultiAttackIntent(WritheDamage, WritheHits));
    }

    private MoveState GetParasolState()
    {
        return new MoveState(PARASOL, Parasol, new DefendIntent(), new BuffIntent());
    }

    private MoveState GetRealityState()
    {
        return new MoveState(REALITY, Reality, new SingleAttackIntent(RealityDamage), new StatusIntent(StatusAmt));
    }
    
    private MoveState GetShriekState()
    {
        return new MoveState(SHRIEK, Shriek, new SingleAttackIntent(ShriekDamage), new DebuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetWritheState();
        var state2 = GetParasolState();
        var state3 = GetRealityState();
        var state4 = GetShriekState();
        
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
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (Enraged)
        {
            if (!LastMove(stateMachine, SHRIEK))
            {
                return SHRIEK;
            }
            else
            {
                return WRITHE;
            }
        }
        else
        {
            if (LastMove(stateMachine, REALITY))
            {
                return WRITHE;
            } else if (LastMove(stateMachine, WRITHE))
            {
                return PARASOL;
            }
            else
            {
                return REALITY;
            }
        }
    }
    
    private async Task Writhe(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < WritheHits; i++)
        {
            if (i % 2 == 0) {
                await PierceAnimation(targets);
            } else {
                await SlashAnimation(targets);
            } 
            await DamageCmd.Attack(WritheDamage)
                .FromMonster(this)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Parasol(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        int numAliveBrothers = 0;
        Dictionary<Brother, int> brotherStrMap = new Dictionary<Brother, int>();
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Monster is Brother brother)
            {
                brotherStrMap[brother] = 1;
                numAliveBrothers++;
            }
        }
        if (numAliveBrothers > 0 && numAliveBrothers < NumActiveBrothers)
        {
            int leftOverStr = NumActiveBrothers - numAliveBrothers;
            while (leftOverStr > 0)
            {
                foreach (var key in brotherStrMap.Keys)
                {
                    if (leftOverStr > 0)
                    {
                        leftOverStr--;
                        brotherStrMap[key] += 1;
                    }
                }
            }
        }
        foreach (var enemy in CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(enemy, BlockAmt, ValueProp.Move, null);
            int str = StrAmt;
            if (enemy.Monster is Brother brother)
            {
                str = brotherStrMap[brother];
            }
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), enemy, str, Creature,  null);
        }
        await ResetIdle();
    }
    
    private async Task Reality(IReadOnlyList<Creature> targets)
    {
        await PierceAnimation(targets);
        await DamageCmd.Attack(RealityDamage)
            .FromMonster(this)
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle();
    }
    
    private async Task Shriek(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await DamageCmd.Attack(ShriekDamage)
            .FromMonster(this)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Erosion>(targets, ErosionAmt);
        await ResetIdle(1.5f);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwanVertDown, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.SwanPierce, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.SwanGuard);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.SwanShout, targets, 0.8f);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Slash", "Pierce", "Block", "Special"], controller);
    }
}