using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Kali;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Patches;
using Ruina2.Ruina2Code.Powers.Act3;
using Bleed = Ruina2.Ruina2Code.Powers.Bleed;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class Kali : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 600, 660);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    private int UpstandingDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int UpstandingHits => 2;
    private int SpearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int SpearHits => 3;
    private int LevelDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int LevelHits => 2;
    private int GSHDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 44, 40);
    private int StrAmt => 3;
    private int StartingStr => 2;
    private int BlockAmt => 12;
    private int LevelBlock => 8;
    private int FrailAmt => 1;
    private int BleedAmt => 3;
    
    private int phase = 2;
    private bool ManifestedEgo => phase == 2;

    protected override string VisualsPath => "Kali/kali.tscn".MonsterImagePath();

    private const string UPSTANDING_SLASH = "UPSTANDING_SLASH";
    private const string SPEAR = "SPEAR";
    private const string LEVEL_SLASH = "LEVEL_SLASH";
    public static string FOCUS_SPIRIT = "FOCUS_SPIRIT";
    public static string GSH = "GSH";
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Creature.CurrentHp = (int)(Creature.MaxHp * 0.5f);
        await PowerCmd.Apply<RedMistKali>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StartingStr, Creature, null);
    }
    
    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // Timing jank
        if (side == CombatSide.Player && CombatState.RoundNumber == 1)
        {
            Sfx.RedMistChange.Play();
            MusicPatches.RuinaActMusicPatches.RedMistManifestEGO();
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
        return new MoveState(LEVEL_SLASH, LevelSlash, new RuinaMultiAttackIntent(LevelDamage, LevelHits), new DefendIntent());
    }
    
    private MoveState GetFocusSpiritState()
    {
        return new MoveState(FOCUS_SPIRIT, FocusSpirit, new DefendIntent(), new BuffIntent());
    }
    
    private MoveState GetGSHState()
    {
        return new MoveState(GSH, GreaterSplitHorizontal, new RuinaSingleAttackIntent(GSHDamage), new RuinaDebuffIntent());
    }
    
    private MonsterMoveStateMachine GenerateIntentStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetUpstandingSlashState();
        var state2 = GetSpearState();
        var state3 = GetLevelSlashState();
        var state4 = GetFocusSpiritState();
        var state5 = GetGSHState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state5;
        state3.FollowUpState = state1;
        state4.FollowUpState = state3;
        state5.FollowUpState = state4;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        
        return new MonsterMoveStateMachine(states, state3);
    }
    
    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntentStateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return GetPlayerTarget();
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<UpstandingSlashKali>();
        card1.SetDamage(UpstandingDamage);
        card1.SetRepeat(UpstandingHits);
        card1.SetFrail(FrailAmt);
        var card2 = CreateCardForIntent<SpearKali>();
        card2.SetDamage(SpearDamage);
        card2.SetRepeat(SpearHits);
        var card3 = CreateCardForIntent<LevelSlashKali>();
        card3.SetDamage(LevelDamage);
        card3.SetRepeat(LevelHits);
        card3.SetBlock(LevelBlock);
        var card4 = CreateCardForIntent<FocusSpiritKali>();
        card4.SetBlock(BlockAmt);
        card4.SetStrength(StrAmt);
        var card5 = CreateCardForIntent<GreaterSplitHorizontalKali>();
        card5.SetDamage(GSHDamage);
        card5.DynamicVars["Bleed"].BaseValue = BleedAmt;
        return new Dictionary<string, CardModel>()
        {
            {UPSTANDING_SLASH, card1},
            {SPEAR, card2},
            {LEVEL_SLASH, card3},
            {FOCUS_SPIRIT, card4},
            {GSH, card5},
        };
    }
    
   private async Task UpstandingSlash(IReadOnlyList<Creature> targets)
    {
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
            foreach (var result in attackCommand.Results.SelectMany(r => r))
            {
                if (result.UnblockedDamage > 0 && result.Receiver.IsPlayer)
                {
                    await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), result.Receiver, FrailAmt, Creature, null);
                }
            }
            await ResetIdle(0.5f, phase);
        }
    }
    
    private async Task Spear(IReadOnlyList<Creature> targets)
    {
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
            foreach (var result in attackCommand.Results.SelectMany(r => r))
            {
                if (result.UnblockedDamage > 0 && result.Receiver.IsPlayer)
                {
                    await CreatureCmd.GainBlock(Creature, LevelBlock, ValueProp.Move, null);
                    break;
                }
            }
            await ResetIdle(0.5f, phase);
        }
    }
    
    private async Task FocusSpirit(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrAmt, Creature, null);
        await ResetIdle(1.0f, phase);
    }
    
    private async Task GreaterSplitHorizontal(IReadOnlyList<Creature> targets)
    {
        await GSHFullScreenAnimation();
        await HorizontalAnimation(targets);
        var attackCommand = await DamageCmd.Attack(GSHDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        foreach (var result in attackCommand.Results.SelectMany(r => r))
        {
            if (result.UnblockedDamage > 0 && result.Receiver.IsPlayer)
            {
                await PowerCmd.Apply<Bleed>(new ThrowingPlayerChoiceContext(), result.Receiver, BleedAmt, Creature, null);
            }
        }
        await ResetIdle(1.0f, phase);
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
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block" + phase, null, null);
    }
    
    private async Task HorizontalAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Horizontal", Sfx.RedMistHoriFin, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle2", "Idle1", "Blunt1", "Blunt2", "Pierce1", "Pierce2", "Slash1", "Slash2", "Horizontal", "VertStart", "VertFinish", "Block2"], controller);
    }
}