using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Gebura;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Patches;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Tanya;

public sealed class Gebura : AbstractAllyCardMonster
{
    public override int MinInitialHp => 200;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "GeburaIcon.png".UIImagePath();
    private bool talked = false;

    private int UpstandingDamage => 7;
    private int UpstandingHits => 2;
    private int SpearDamage => 4;
    private int SpearHits => 3;
    private int LevelDamage => 6;
    private int LevelHits => 2;
    private int GSVDamage => 30;
    private int GSHDamage => 40;
    private int LevelStrength => 1;
    private int UpstandingVulnerable => 1;
    private int PowerStrength => 1;
    private int phase = 1;
    private int TurnCounter = 4;
    private bool ManifestedEgo => phase == 2;
    protected override string VisualsPath => "Gebura/gebura.tscn".MonsterImagePath();

    private const string UPSTANDING_SLASH = "UPSTANDING_SLASH";
    private const string SPEAR = "SPEAR";
    private const string LEVEL_SLASH = "LEVEL_SLASH";
    public static string GSV = "GSV";
    public static string GSH = "GSH";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Tanya>();
        var power = await PowerCmd.Apply<RedMist>(new ThrowingPlayerChoiceContext(), Creature, PowerStrength, Creature,  null);
        if (power != null)
        {
            power.DynamicVars["Turns"].BaseValue = TurnCounter;
        }
    }

    private MoveState GetUpstandingSlashState()
    {
        return new MoveState(UPSTANDING_SLASH, UpstandingSlash, new RuinaMultiAttackIntent(UpstandingDamage, UpstandingHits), new RuinaDebuffIntent());
    }

    private MoveState GetSpearState()
    {
        return new MoveState(SPEAR, Spear, new RuinaMultiAttackIntent(SpearDamage, SpearHits));
    }
    
    private MoveState GetLevelSlashState()
    {
        return new MoveState(LEVEL_SLASH, LevelSlash, new RuinaMultiAttackIntent(LevelDamage, LevelHits), new RuinaBuffIntent());
    }
    
    private MoveState GetGSVState()
    {
        return new MoveState(GSV, GreaterSplitVertical, new RuinaSingleAttackIntent(GSVDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetGSHState()
    {
        return new MoveState(GSH, GreaterSplitHorizontal, new RuinaSingleAttackIntent(GSHDamage), new RuinaDebuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetUpstandingSlashState();
        var state2 = GetSpearState();
        var state3 = GetLevelSlashState();
        var state4 = GetGSVState();
        var state5 = GetGSHState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

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

    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (ManifestedEgo && ThreeTurnCooldownHasPassedForMove(stateMachine, GSH) && ThreeTurnCooldownHasPassedForMove(stateMachine, GSV))
        {
            return GSH;
        } else if (!ManifestedEgo && ThreeTurnCooldownHasPassedForMove(stateMachine, GSV))
        {
            return GSV;
        }
        else
        {
            if (stateMachine.StateLog.Count >= 3)
            {
                stateMachine.StateLog.Clear();
            }
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, UPSTANDING_SLASH) && !LastMoveBefore(stateMachine, UPSTANDING_SLASH)) {
                possibilities.Add(UPSTANDING_SLASH);
            }
            if (!LastMove(stateMachine, SPEAR) && !LastMoveBefore(stateMachine, SPEAR)) {
                possibilities.Add(SPEAR);
            }
            if (!LastMove(stateMachine, LEVEL_SLASH) && !LastMoveBefore(stateMachine, LEVEL_SLASH)) {
                possibilities.Add(LEVEL_SLASH);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        if (OtherSideTargetMonster != null)
        {
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<UpstandingSlash>();
        card1.SetDamage(UpstandingDamage);
        card1.SetRepeat(UpstandingHits);
        card1.SetVulnerable(UpstandingVulnerable);
        var card2 = CreateCardForIntent<Spear>();
        card2.SetDamage(SpearDamage);
        card2.SetRepeat(SpearHits);
        var card3 = CreateCardForIntent<LevelSlash>();
        card3.SetDamage(LevelDamage);
        card3.SetRepeat(LevelHits);
        card3.SetStrength(LevelStrength);
        var card4 = CreateCardForIntent<GreaterSplitVertical>();
        card4.SetDamage(GSVDamage);
        var card5 = CreateCardForIntent<GreaterSplitHorizontal>();
        card5.SetDamage(GSHDamage);
        return new Dictionary<string, CardModel>()
        {
            {UPSTANDING_SLASH, card1},
            {SPEAR, card2},
            {LEVEL_SLASH, card3},
            {GSV, card4},
            {GSH, card5},
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-GEBURA.response"), Creature, VfxColor.Red);
            talked = true;
        }
    }

    private async Task UpstandingSlash(IReadOnlyList<Creature> targets)
    {
        Talk();
        List<AttackCommand> attackResults = new List<AttackCommand>();
        for (int i = 0; i < UpstandingHits; i++)
        {
            if (i % 2 == 0)
            {
                await SlashAnimation(targets);
            } 
            else
            {
                await BluntAnimation(targets);
            }
            var attackCommand = await DamageCmd.Attack(UpstandingDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            attackResults.Add(attackCommand);
            await ResetIdle(0.5f, phase);
        }
        foreach (var attackResult in attackResults)
        {
            foreach (var result in attackResult.Results.SelectMany(r => r))
            {
                if (result.UnblockedDamage > 0)
                {
                    await ApplyPowerAndSkipNextDurationTickIfNotPresent<VulnerablePower>(targets, UpstandingVulnerable);
                }
                break;
            }
        }
    }
    
    private async Task Spear(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < SpearHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(SpearDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.5f, phase);
        }
    }
    
    private async Task LevelSlash(IReadOnlyList<Creature> targets)
    {
        Talk();
        List<AttackCommand> attackResults = new List<AttackCommand>();
        for (int i = 0; i < LevelHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            var attackCommand = await DamageCmd.Attack(LevelDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            attackResults.Add(attackCommand);
            await ResetIdle(0.5f, phase);
        }
        foreach (var attackResult in attackResults)
        {
            foreach (var result in attackResult.Results.SelectMany(r => r))
            {
                if (result.UnblockedDamage > 0)
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, LevelStrength, Creature,  null); 
                }
                break;
            }
        }
    }
    
    private async Task GreaterSplitVertical(IReadOnlyList<Creature> targets)
    {
        Talk();
        await VerticalUpAnimation(targets);
        await WaitAnimation(0.25f);
        await GSVFullScreenAnimation();
        await VerticalDownAnimation(targets);
        var attackCommand = await DamageCmd.Attack(GSVDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        foreach (var result in attackCommand.Results.SelectMany(r => r))
        {
            if (result.UnblockedDamage > 0)
            {
                await PowerCmd.Apply<GsvTempStrLoss>(new ThrowingPlayerChoiceContext(), result.Receiver, result.UnblockedDamage, Creature,  null); 
            }
            break;
        }
        await ResetIdle(1.0f, phase);
    }
    
    private async Task GreaterSplitHorizontal(IReadOnlyList<Creature> targets)
    {
        Talk();
        await GSHFullScreenAnimation();
        await HorizontalAnimation(targets);
        var attackCommand = await DamageCmd.Attack(GSHDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        foreach (var result in attackCommand.Results.SelectMany(r => r))
        {
            if (result.UnblockedDamage > 0)
            {
                await PowerCmd.Apply<GshTempStrLoss>(new ThrowingPlayerChoiceContext(), result.Receiver, result.UnblockedDamage, Creature,  null); 
            }
            break;
        }
        await ResetIdle(1.0f, phase);
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-GEBURA.victory"), Creature, VfxColor.Red);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }
    
    public override async Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Creature))
        {
            if (!ManifestedEgo)
            {
                TurnCounter--;
                if (TurnCounter == 0)
                {
                    phase = 2;
                    Sfx.RedMistChange.Play();
                    MusicPatches.RuinaActMusicPatches.RedMistManifestEGO();
                    await ResetIdle(0.0f, phase);
                    var strAmt = Creature.GetPowerAmount<StrengthPower>();
                    if (strAmt > 0)
                    {
                        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, strAmt, Creature,  null); 
                    }
                }
            }
        }
    }
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer == Creature && target != null && target.Block == 0 && NextMoves.Count > 0 && NextMoves[0].Id == SPEAR)
        {
            return 2M;
        }
        return 1M;
    }
    
    private async Task GSVFullScreenAnimation()
    {
        Sfx.RedMistVertCut.Play();
        var fullScreenEffect = FullScreenAnimationEffect.Create("VerticalSplit/frame".VfxImagePath(), 1.0f, 10);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        await WaitAnimation(1.0f);
    }
    
    private async Task GSHFullScreenAnimation()
    {
        Sfx.RedMistHoriEye.Play();
        Sfx.RedMistHoriStart.Play();
        var fullScreenEffect = FullScreenAnimationEffect.Create("HorizontalSplit/frame".VfxImagePath(), 1.2f, 12);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        await WaitAnimation(1.2f);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash" + phase, ManifestedEgo ? Sfx.RedMistVert2 : Sfx.RedMistVert1, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce" + phase, ManifestedEgo ? Sfx.RedMistStab2 : Sfx.RedMistStab1, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt" + phase, ManifestedEgo ? Sfx.RedMistHori2 : Sfx.RedMistHori1, targets);
    }
    
    private async Task VerticalUpAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("VertStart", Sfx.RedMistVertHit, targets);
    }
    
    private async Task VerticalDownAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("VertFinish", Sfx.RedMistVertFin, targets);
    }
    
    private async Task HorizontalAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Horizontal", Sfx.RedMistHoriFin, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Blunt1", "Blunt2", "Pierce1", "Pierce2", "Slash1", "Slash2", "Horizontal", "VertStart", "VertFinish"], controller);
    }
}