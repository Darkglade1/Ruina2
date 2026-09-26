using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Malkuth;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Philip;

public sealed class Malkuth : AbstractAllyCardMonster
{
    public override int MinInitialHp => 150;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "MalkuthIcon.png".UIImagePath();
    private bool talked = false;

    private int TurbulenceDamage => 16;
    private int EmotionsDamage => 12;
    private int EmotionsHits => 2;
    private int StormDamage => 20;
    private int StormHits => 2;
    private int InfernoDamage => 70;
    private int SelfBlockAmt => 14;
    private int AllyBlockAmt => 14;
    private int AllyStrengthAmt => 2;
    private int StormVulnAmt => 2;
    private int CardDraw => 1;
    public int FervidStrengthAmt = 2;
    public int EmotionalStrengthAmt = 1;
    public static int FirstStrengthThreshold = 10;
    public static int SecondStrengthThreshold = 20;
    public static int ExhaustStrengthGain = 1;
    private int phase = 1;
    private bool distorted;
    private bool manifestedEGO;
    private bool usedRagingStorm;
    private bool usedInferno;
    protected override string VisualsPath => "Malkuth/malkuth.tscn".MonsterImagePath();

    private const string COORDINATED_ASSAULT = "COORDINATED_ASSAULT";
    private const string EMOTIONAL_TURBULENCE = "EMOTIONAL_TURBULENCE";
    private const string FERVID_EMOTIONS = "FERVID_EMOTIONS";
    private const string RAGING_STORM = "RAGING_STORM";
    private const string INFERNO = "INFERNO";
    

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Philip>();
        foreach (Creature target in CombatState.PlayerCreatures)
        {
            Dragon mutable = (Dragon) ModelDb.Power<Dragon>().ToMutable();
            mutable.Target = target;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), mutable, Creature, 3, Creature, null);
        }
        var emotion = await PowerCmd.Apply<Emotion>(new ThrowingPlayerChoiceContext(), Creature, CombatState.PlayerCreatures.Count, Creature,  null);
        if (emotion != null)
        {
            emotion.DynamicVars["StrengthAmount"].BaseValue = ExhaustStrengthGain;
            emotion.DynamicVars["FirstStrengthThreshold"].BaseValue = FirstStrengthThreshold;
            emotion.DynamicVars["SecondStrengthThreshold"].BaseValue = SecondStrengthThreshold;
        }
    }

    private MoveState GetCoordinatedAssaultState()
    {
        return new MoveState(COORDINATED_ASSAULT, CoordinatedAssault, new RuinaDefendIntent(), new RuinaBuffIntent());
    }

    private MoveState GetEmotionalTurbulenceState()
    {
        return new MoveState(EMOTIONAL_TURBULENCE, EmotionalTurbulence, new RuinaSingleAttackIntent(TurbulenceDamage), new RuinaDefendIntent());
    }
    
    private MoveState GetFervidEmotionsState()
    {
        return new MoveState(FERVID_EMOTIONS, FervidEmotions, new RuinaMultiAttackIntent(EmotionsDamage, EmotionsHits), new RuinaBuffIntent());
    }
    
    private MoveState GetRagingStormState()
    {
        return new MoveState(RAGING_STORM, RagingStorm, new RuinaMultiMassAttackIntent(StormDamage, StormHits), new RuinaDebuffIntent());
    }
    
    private MoveState GetInfernoState()
    {
        return new MoveState(INFERNO, Inferno, new RuinaSingleMassAttackIntent(InfernoDamage));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetCoordinatedAssaultState();
        var state2 = GetEmotionalTurbulenceState();
        var state3 = GetFervidEmotionsState();
        var state4 = GetRagingStormState();
        var state5 = GetInfernoState();
        
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
        if (distorted && !usedRagingStorm)
        {
            return RAGING_STORM;
        } else if (manifestedEGO && !usedInferno)
        {
            return INFERNO;
        }
        else
        {
            List<string> possibilities = new List<string>();
            if (!distorted && !manifestedEGO)
            {
                if (!LastMove(stateMachine, COORDINATED_ASSAULT) && !LastMoveBefore(stateMachine, COORDINATED_ASSAULT)) {
                    possibilities.Add(COORDINATED_ASSAULT);
                }
            }
            if (!LastMove(stateMachine, EMOTIONAL_TURBULENCE)) {
                possibilities.Add(EMOTIONAL_TURBULENCE);
            }
            if (!LastMove(stateMachine, FERVID_EMOTIONS)) {
                possibilities.Add(FERVID_EMOTIONS);
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
        var card1 = CreateCardForIntent<FervidEmotions>();
        card1.SetDamage(EmotionsDamage);
        card1.SetRepeat(EmotionsHits);
        card1.SetStrength(FervidStrengthAmt);
        var card2 = CreateCardForIntent<CoordinatedAssault>();
        card2.SetBlock(AllyBlockAmt);
        card2.SetCards(CardDraw);
        card2.SetStrength(AllyStrengthAmt);
        var card3 = CreateCardForIntent<EmotionalTurbulence>();
        card3.SetDamage(TurbulenceDamage);
        card3.SetBlock(SelfBlockAmt);
        card3.SetStrength(EmotionalStrengthAmt);
        var card4 = CreateCardForIntent<RagingStorm>();
        card4.SetDamage(StormDamage);
        card4.SetRepeat(StormHits);
        card4.SetVulnerable(StormVulnAmt);
        var card5 = CreateCardForIntent<Inferno>();
        card5.SetDamage(InfernoDamage);
        return new Dictionary<string, CardModel>()
        {
            {FERVID_EMOTIONS, card1},
            {COORDINATED_ASSAULT, card2},
            {EMOTIONAL_TURBULENCE, card3},
            {RAGING_STORM, card4},
            {INFERNO, card5}
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-MALKUTH.response"), Creature, VfxColor.Gold);
            talked = true;
        }
    }

    private async Task CoordinatedAssault(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BlockAnimation();
        foreach (var player in CombatState.PlayerCreatures)
        {
            await CreatureCmd.GainBlock(player, AllyBlockAmt, ValueProp.Move, null);
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, AllyStrengthAmt, Creature,  null); 
        await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, CardDraw, Creature,  null); 
        await ResetIdle(1.0f, phase);
    }
    
    private async Task EmotionalTurbulence(IReadOnlyList<Creature> targets)
    {
        Talk();
        await SlashAnimation(targets);
        await AllyBlock(SelfBlockAmt);
        await DamageCmd.Attack(EmotionsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, EmotionalStrengthAmt, Creature,  null); 
        await ResetIdle(0.5f, phase);
    }
    
    private async Task FervidEmotions(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < EmotionsHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(EmotionsDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.5f, phase);
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, FervidStrengthAmt, Creature,  null); 
    }
    
    private async Task RagingStorm(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < StormHits; i++)
        {
            IsMassAttacking = true;
            if (i % 2 == 0) {
                await RagingStormStartAnimation(targets);
            } else {
                await RagingStormFinAnimation(targets);
            }
            await DamageCmd.Attack(StormDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
            await ResetIdle(1.0f, phase);
        }
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy != Creature)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), enemy, StormVulnAmt, Creature,  null); 
            }
        }
        usedRagingStorm = true;
    }
    
    private async Task Inferno(IReadOnlyList<Creature> targets)
    {
        IsMassAttacking = true;
        await InfernoStartAnimation(targets);
        await WaitAnimation(1.0f);
        await InfernoFinAnimation(targets);
        NCombatRoom? instance = NCombatRoom.Instance;
        foreach (Creature hittableEnemy in CombatState.HittableEnemies)
        {
            if (hittableEnemy != Creature)
            {
                NFireBurstVfx? child = NFireBurstVfx.Create(hittableEnemy, 0.75f);
                if (instance != null)
                {
                    instance.CombatVfxContainer.AddChildSafely(child);
                }
            }
        }
        await DamageCmd.Attack(InfernoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
        await ResetIdle(1.0f, phase);
        usedInferno = true;
    }
    
    public override IReadOnlyList<Creature> AdditionalMassAttackTargets()
    {
        var newList = new List<Creature>();
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            if (hittableEnemy != Creature && hittableEnemy != OtherSideTargetMonster)
            {
                newList.Add(hittableEnemy);
            }
        }
        return newList;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Creature))
        {
            int strAmt = Creature.GetPowerAmount<StrengthPower>();
            if (strAmt >= FirstStrengthThreshold && !distorted)
            {
                distorted = true;
                phase++;
                await ResetIdle(0.0f, phase);
            } else if (strAmt >= SecondStrengthThreshold && distorted && !manifestedEGO)
            {
                manifestedEGO = true;
                phase++;
                await ResetIdle(0.0f, phase);
                Sfx.XiaoRoar.Play();
                await WaitAnimation();
                if (NCombatRoom.Instance != null)
                {
                    NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
                    creatureNode?.MoveChildSafely(creatureNode?.Visuals, 0);
                    Marker2D? specialNode = creatureNode?.GetSpecialNode<Marker2D>("%IntentPos");
                    if (specialNode != null)
                    {
                        specialNode.Position += new Vector2(0.0f, -50.0f);
                    }
                }
            }
        }
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-MALKUTH.victory"), Creature, VfxColor.Gold);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash" + phase, Sfx.XiaoVert, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce" + phase, Sfx.XiaoStab, targets);
    }
    
    private async Task RagingStormStartAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash" + phase, Sfx.XiaoStrongStart, targets);
    }
    
    private async Task RagingStormFinAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special2", Sfx.XiaoStrongFin, targets);
    }
    
    private async Task InfernoStartAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special3", Sfx.XiaoStart, targets);
    }
    
    private async Task InfernoFinAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special4", Sfx.XiaoFin, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Guard" + phase, Sfx.FireGuard);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle1", "Idle2", "Idle3", "Pierce1", "Pierce2", "Pierce3", "Slash1", "Slash2", "Slash3", "Guard1", "Guard2", "Guard3", "Special2", "Special3", "Special4"], controller);
    }
}