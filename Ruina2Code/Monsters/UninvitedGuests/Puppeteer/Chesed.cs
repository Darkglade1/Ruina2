using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Chesed;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Puppeteer;

public sealed class Chesed : AbstractAllyCardMonster
{
    public override int MinInitialHp => 150;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "ChesedIcon.png".UIImagePath();
    private bool talked = false;

    private int BattlefieldDamage => 18;
    private int EnergyShieldDamage => 7;
    private int ConcentrateDamage => 11;
    private int ConcentrateHits => 2;
    private int DisposalDamage => 12;
    private int DisposalHits => 2;
    private int AllyBlockAmt => 15;
    private int SelfBlockAmt => 18;
    private int StrengthAmt => 2;
    private float DisposalHPThreshold => 0.25f;
    public MoveState? _disposalState;
    public MoveState? DisposalState
    {
        get => _disposalState;
        set
        {
            AssertMutable();
            _disposalState = value;
        }
    }
    protected override string VisualsPath => "Chesed/chesed.tscn".MonsterImagePath();

    private const string BATTLEFIELD_COMMAND = "BATTLEFIELD_COMMAND";
    private const string ENERGY_SHIELD = "ENERGY_SHIELD";
    private const string CONCENTATE = "CONCENTATE";
    private const string DISPOSAL = "DISPOSAL";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Puppeteer>();
        await PowerCmd.Apply<FinishingTouch>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetBattlefieldCommandState()
    {
        return new MoveState(BATTLEFIELD_COMMAND, BattlefieldCommand, new RuinaSingleAttackIntent(BattlefieldDamage), new RuinaBuffIntent());
    }

    private MoveState GetEnergyShieldState()
    {
        return new MoveState(ENERGY_SHIELD, EnergyShield, new RuinaSingleAttackIntent(EnergyShieldDamage), new RuinaDefendIntent());
    }
    
    private MoveState GetConcentrateState()
    {
        return new MoveState(CONCENTATE, Concentrate, new RuinaMultiAttackIntent(ConcentrateDamage, ConcentrateHits), new RuinaDefendIntent());
    }
    
    private MoveState GetDisposalState()
    {
        return new MoveState(DISPOSAL, Disposal, new RuinaMultiAttackIntent(DisposalDamage, DisposalHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBattlefieldCommandState();
        var state2 = GetEnergyShieldState();
        var state3 = GetConcentrateState();
        DisposalState = GetDisposalState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        DisposalState.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(DisposalState);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }

    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (OtherSideTargetMonster != null && OtherSideTargetMonster.CurrentHp <= (int)(OtherSideTargetMonster.MaxHp * DisposalHPThreshold)) {
            if (OtherSideTargetMonster.Monster is Puppeteer puppeteer && (puppeteer.puppet == null || (puppeteer.puppet.IsDead && puppeteer.puppet.Monster is AbstractMultiIntentMonster multiIntentPuppet && multiIntentPuppet.NextMoves[0].Id == Puppet.REVIVING))) {
                if (OtherSideTargetMonster.HasPower<Mark>()) {
                    TalkCmd.Play(L10NMonsterLookup("RUINA2-CHESED.disposal"), Creature, VfxColor.Blue);
                    return DISPOSAL;
                }
            }
        }
        if (stateMachine.StateLog.Count >= 3)
        {
            stateMachine.StateLog.Clear();
        }
        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, BATTLEFIELD_COMMAND) && !LastMoveBefore(stateMachine, BATTLEFIELD_COMMAND)) {
            possibilities.Add(BATTLEFIELD_COMMAND);
        }
        if (!LastMove(stateMachine, ENERGY_SHIELD) && !LastMoveBefore(stateMachine, ENERGY_SHIELD)) {
            possibilities.Add(ENERGY_SHIELD);
        }
        if (!LastMove(stateMachine, CONCENTATE) && !LastMoveBefore(stateMachine, CONCENTATE)) {
            possibilities.Add(CONCENTATE);
        }
        return possibilities[rng.NextInt(possibilities.Count)];
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null)
        {
            if (OtherSideTargetMonster.Monster is Puppeteer puppeteer && puppeteer.puppet != null && puppeteer.puppet.IsAlive)
            {
                return puppeteer.puppet;
            }
            else
            {
                return OtherSideTargetMonster;
            }
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Concentrate>();
        card1.SetDamage(ConcentrateDamage);
        card1.SetRepeat(ConcentrateHits);
        card1.SetBlock(SelfBlockAmt);
        var card2 = CreateCardForIntent<EnergyShield>();
        card2.SetBlock(AllyBlockAmt);
        card2.SetDamage(EnergyShieldDamage);
        var card3 = CreateCardForIntent<BattlefieldCommand>();
        card3.SetDamage(BattlefieldDamage);
        card3.SetStrength(StrengthAmt);
        var card4 = CreateCardForIntent<Disposal>();
        card4.SetDamage(DisposalDamage);
        card4.SetRepeat(DisposalHits);
        return new Dictionary<string, CardModel>()
        {
            {CONCENTATE, card1},
            {ENERGY_SHIELD, card2},
            {BATTLEFIELD_COMMAND, card3},
            {DISPOSAL, card4},
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-CHESED.response"), Creature, VfxColor.Blue);
            talked = true;
        }
    }

    private async Task BattlefieldCommand(IReadOnlyList<Creature> targets)
    {
        Talk();
        await SlashAnimation(targets);
        await DamageCmd.Attack(BattlefieldDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmt, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, StrengthAmt, Creature,  null);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Mark>(targets, 1);
        await ResetIdle();
    }
    
    private async Task EnergyShield(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, AllyBlockAmt, ValueProp.Move, null);
        foreach (var player in CombatState.PlayerCreatures)
        {
            await CreatureCmd.GainBlock(player, AllyBlockAmt, ValueProp.Move, null);
        }
        await WaitAnimation();
        await PierceAnimation(targets);
        await DamageCmd.Attack(EnergyShieldDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Mark>(targets, 1);
        await ResetIdle();
    }
    
    private async Task Concentrate(IReadOnlyList<Creature> targets)
    {
        Talk();
        await CreatureCmd.GainBlock(Creature, SelfBlockAmt, ValueProp.Move, null);
        for (int i = 0; i < ConcentrateHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(ConcentrateDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await WaitAnimation();
        }
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Mark>(targets, 1);
        await ResetIdle();
    }
    
    private async Task Disposal(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < DisposalHits; i++)
        {
            if (i % 2 == 0)
            {
                await DisposalFinishAnimation(targets);
            } 
            else
            {
                await DisposalUpAnimation(targets);
                await WaitAnimation();
                await DisposalDownAnimation(targets);
            }
            await DamageCmd.Attack(DisposalDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await WaitAnimation(1.0f);
        }
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Mark>(targets, 1);
        await ResetIdle();
    }
    
    // public override async Task AfterSideTurnEnd(
    //     PlayerChoiceContext choiceContext,
    //     CombatSide side,
    //     IEnumerable<Creature> participants)
    // {
    //     if (participants.Contains(Creature))
    //     {
    //         if (OtherSideTargetMonster != null && OtherSideTargetMonster.CurrentHp <= (int)(OtherSideTargetMonster.MaxHp * DisposalHPThreshold)) {
    //             if (OtherSideTargetMonster.Monster is Puppeteer puppeteer && (puppeteer.puppet == null || (puppeteer.puppet.IsDead && puppeteer.puppet.Monster is AbstractMultiIntentMonster multiIntentPuppet && multiIntentPuppet.NextMoves[0].Id == Puppet.REVIVING))) {
    //                 if (OtherSideTargetMonster.HasPower<Mark>()) {
    //                     TalkCmd.Play(L10NMonsterLookup("RUINA2-CHESED.disposal"), Creature, VfxColor.Blue);
    //                     if (DisposalState != null)
    //                     {
    //                         SetMoveImmediateMultiIntentMonster(DisposalState, 0);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        decimal multiplier = 1M;
        if (dealer == Creature && dealer.Monster is Chesed chesed && props.IsPoweredAttack())
        {
            if (chesed.NextMoves.Count > 0 && chesed.NextMoves[0].Id == DISPOSAL)
            {
                if (target != null)
                {
                    if (target.HasPower<Mark>())
                    {
                        multiplier *= 2M;
                    }

                    if (target.CurrentHp <= (int)(target.MaxHp * DisposalHPThreshold))
                    {
                        multiplier *= 2M;
                    }
                }
                return multiplier;
            }
        }
        return 1M;
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-CHESED.victory"), Creature, VfxColor.Blue);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.SwordVert, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.SwordStab, targets);
    }
    
    private async Task DisposalUpAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special3", Sfx.DisposalUp, targets);
    }
    
    private async Task DisposalDownAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special4", Sfx.DisposalDown, targets);
    }
    
    private async Task DisposalFinishAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special2", Sfx.DisposalFinish, targets);
        await AnimationAction("Special2", Sfx.DisposalBlood, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Slash", "Block", "Special2", "Special3", "Special4"], controller);
    }
}