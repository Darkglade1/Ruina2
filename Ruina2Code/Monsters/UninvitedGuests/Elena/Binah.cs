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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Binah;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Elena;

public sealed class Binah : AbstractAllyCardMonster
{
    public override int MinInitialHp => 120;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "BinahIcon.png".UIImagePath();
    private bool talked = false;

    private int PillarDamage => 24;
    private int FairyDamage => 14;
    private int FairyHits => 2;
    private int ChainsDamage => 21;
    private int DebuffAmt => 1;
    private int BlockAmt => 14;
    
    protected override string VisualsPath => "Binah/binah.tscn".MonsterImagePath();

    private const string PILLAR = "PILLAR";
    private const string CHAINS = "CHAINS";
    private const string FAIRY = "FAIRY";

    private Creature? elena;
    private Creature? vermilion;

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Elena>();
        elena = FindTarget<Elena>();
        vermilion = FindTarget<Vermilion>();
        await PowerCmd.Apply<ArbitersJudgement>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
    }

    private MoveState GetPillarState()
    {
        return new MoveState(PILLAR, Pillar, new RuinaSingleAttackIntent(PillarDamage), new RuinaDefendIntent());
    }

    private MoveState GetChainsState()
    {
        return new MoveState(CHAINS, Chains, new RuinaSingleAttackIntent(ChainsDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetFairyState()
    {
        return new MoveState(FAIRY, Fairy, new RuinaMultiAttackIntent(FairyDamage, FairyHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPillarState();
        var state2 = GetChainsState();
        var state3 = GetFairyState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

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
        if (stateMachine.StateLog.Count >= 3)
        {
            stateMachine.StateLog.Clear();
        }
        List<string> possibilities = new List<string>();
        if (!LastMove(stateMachine, PILLAR) && !LastMoveBefore(stateMachine, PILLAR)) {
            possibilities.Add(PILLAR);
        }
        if (!LastMove(stateMachine, CHAINS) && !LastMoveBefore(stateMachine, CHAINS)) {
            possibilities.Add(CHAINS);
        }
        if (!LastMove(stateMachine, FAIRY) && !LastMoveBefore(stateMachine, FAIRY)) {
            possibilities.Add(FAIRY);
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
            return OtherSideTargetMonster;
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Fairy>();
        card1.SetDamage(FairyDamage);
        card1.SetRepeat(FairyHits);
        var card2 = CreateCardForIntent<Chains>();
        card2.SetDamage(ChainsDamage);
        card2.SetWeak(DebuffAmt);
        var card3 = CreateCardForIntent<Pillar>();
        card3.SetDamage(PillarDamage);
        card3.SetBlock(BlockAmt);
        return new Dictionary<string, CardModel>()
        {
            {FAIRY, card1},
            {CHAINS, card2},
            {PILLAR, card3},
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-BINAH.response"), Creature, VfxColor.Black);
            talked = true;
        }
    }

    private async Task Pillar(IReadOnlyList<Creature> targets)
    {
        Talk();
        await SpecialAnimation(targets);
        await AllyBlock(BlockAmt);
        await WaitAnimation(0.25f);
        await PillarAnimation();
        await DamageCmd.Attack(PillarDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
        if (targets.Count > 0 && targets[0].Monster is AbstractMultiIntentMonster monster)
        {
            monster.CancelIntent();
        }
    }
    
    private async Task Chains(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BluntAnimation(targets);
        await DamageCmd.Attack(ChainsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, DebuffAmt, Creature,  null); 
        await ResetIdle();
        if (targets.Count > 0 && targets[0].Monster is AbstractMultiIntentMonster monster)
        {
            monster.CancelIntent();
        }
    }
    
    private async Task Fairy(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < FairyHits; i++)
        {
            if (i % 2 == 0)
            {
                await BluntAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(FairyDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        if (targets.Count > 0 && targets[0].Monster is AbstractMultiIntentMonster monster)
        {
            monster.CancelIntent();
        }
    }
    
    public async Task OnBossDeath()
    {
        if ((elena == null || elena.IsDead) && (vermilion == null || vermilion.IsDead))
        {
            SetToSide(CombatSide.Enemy);
            await ResetIdle(0.5f);
            TalkCmd.Play(L10NMonsterLookup("RUINA2-BINAH.victory"), Creature, VfxColor.Black);
            await WaitAnimation(2.0f);
            await CreatureCmd.Kill(Creature);
        }
    }
    
    private async Task PillarAnimation()
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(OtherSideTargetMonster);
        if (node != null && targetNode != null)
        {
            var pillarEffect = PillarEffect.Create(node.VfxSpawnPosition, targetNode.GlobalPosition.X);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(pillarEffect);
            await WaitAnimation(0.8f);
        }
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BinahFairy, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BinahChain, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.BinahStoneReady, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Slash", "Special"], controller);
    }
}