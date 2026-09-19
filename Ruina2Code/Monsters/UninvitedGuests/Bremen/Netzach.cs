using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Netzach;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Bremen;

public sealed class Netzach : AbstractAllyCardMonster
{
    public override int MinInitialHp => 160;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "NetzachIcon.png".UIImagePath();
    private bool talked = false;

    private int BalefulDamage => 17;
    private int BlindFaithDamage => 13;
    private int BlindFaithHits => 2;
    private int ErosionAmt => 4;
    private int BlockAmt => 13;
    private int CardDraw => 2;
    protected override string VisualsPath => "Netzach/netzach.tscn".MonsterImagePath();

    private const string WILL = "WILL";
    private const string BALEFUL = "BALEFUL";
    private const string BLIND_FAITH = "BLIND_FAITH";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Bremen>();
        //await PowerCmd.Apply<Geon>(new ThrowingPlayerChoiceContext(), Creature, GEON, Creature,  null);
    }

    private MoveState GetBalefulState()
    {
        return new MoveState(BALEFUL, Baleful, new RuinaSingleAttackIntent(BalefulDamage), new RuinaDebuffIntent());
    }

    private MoveState GetWillState()
    {
        return new MoveState(WILL, Will, new DefendIntent(), new RuinaBuffIntent());
    }
    
    private MoveState GetBlindFaithState()
    {
        return new MoveState(BLIND_FAITH, BlindFaith, new RuinaMultiAttackIntent(BlindFaithDamage, BlindFaithHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBalefulState();
        var state2 = GetWillState();
        var state3 = GetBlindFaithState();
        
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
        if (!LastMove(stateMachine, BALEFUL) && !LastMoveBefore(stateMachine, BALEFUL)) {
            possibilities.Add(BALEFUL);
        }
        if (!LastMove(stateMachine, BLIND_FAITH) && !LastMoveBefore(stateMachine, BLIND_FAITH)) {
            possibilities.Add(BLIND_FAITH);
        }
        if (!LastMove(stateMachine, WILL) && !LastMoveBefore(stateMachine, WILL)) {
            possibilities.Add(WILL);
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
        var card1 = CreateCardForIntent<BlindFaith>();
        card1.SetDamage(BlindFaithDamage);
        card1.SetRepeat(BlindFaithHits);
        var card2 = CreateCardForIntent<Will>();
        card2.SetBlock(BlockAmt);
        card2.SetCards(CardDraw);
        var card3 = CreateCardForIntent<Baleful>();
        card3.SetDamage(BalefulDamage);
        card3.DynamicVars["Erosion"].BaseValue = ErosionAmt;
        return new Dictionary<string, CardModel>()
        {
            {BLIND_FAITH, card1},
            {WILL, card2},
            {BALEFUL, card3},
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-NETZACH.response"), Creature, VfxColor.Green);
            talked = true;
        }
    }

    private async Task Baleful(IReadOnlyList<Creature> targets)
    {
        Talk();
        await SpecialAnimation(targets);
        await DamageCmd.Attack(BalefulDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<Erosion>(targets, ErosionAmt);
        await ResetIdle();
    }
    
    private async Task Will(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        foreach (var player in CombatState.PlayerCreatures)
        {
            await CreatureCmd.GainBlock(player, BlockAmt, ValueProp.Move, null);
        }
        await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, CardDraw, Creature,  null); 
        await ResetIdle(1.0f);
    }
    
    private async Task BlindFaith(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < BlindFaithHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            } 
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(BlindFaithDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await WaitAnimation();
        }
        await ResetIdle();
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-NETZACH.victory"), Creature, VfxColor.Gold);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.YanVert, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.YanStab, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.YanBrand, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", null);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Pierce", "Slash", "Block", "Special"], controller);
    }
}