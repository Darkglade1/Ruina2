using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.Contracts;
using Ruina2.Ruina2Code.Cards.EnemyCards.Pluto;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Pluto;

public sealed class Pluto : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 1300, 1200);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 2;

    private int MissileDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int MissileHits => 3;
    private int OnslaughtDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 25);
    private int OnslaughtDamageIncrease => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int CurrentDamageIncrease = 0;
    public decimal OnslaughtTotalDamage => OnslaughtDamage + CurrentDamageIncrease;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int StatusAmt => 3;
    private int BlockAmt => 30;

    protected override string VisualsPath => "Pluto/pluto.tscn".MonsterImagePath();

    private const string SAFEGUARD = "SAFEGUARD";
    private const string MISSILE = "MISSLE";
    private const string ONSLAUGHT  = "ONSLAUGHT";
    private const string CONTRACT = "CONTRACT";
    private const string BINDING_TERMS = "BINDING_TERMS";
    
    public static readonly IReadOnlyList<IChoosable> ContractSet =
    [
        ModelDb.Card<ContractLiberty>(),
        ModelDb.Card<ContractLight>(),
        ModelDb.Card<ContractMight>(),
    ];

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Hokma>();
        TalkCmd.Play(L10NMonsterLookup("RUINA2-PLUTO.talk"), Creature, VfxColor.DarkGray);
    }

    private MoveState GetSafeguardState()
    {
        return new MoveState(SAFEGUARD, Safeguard, new DefendIntent(), new BuffIntent());
    }

    private MoveState GetMissileState()
    {
        return new MoveState(MISSILE, Missile, new RuinaMultiAttackIntent(MissileDamage, MissileHits));
    }

    private MoveState GetBindingTermsState()
    {
        return new MoveState(BINDING_TERMS, BindingTerms, new StatusIntent(StatusAmt));
    }
    
    private MoveState GetContractState()
    {
        return new MoveState(CONTRACT, Contract, new RuinaDebuffIntent());
    }
    
    private MoveState GetOnslaughtState()
    {
        return new MoveState(ONSLAUGHT, Onslaught, new RuinaSingleAttackIntent((Func<decimal>) (() => OnslaughtTotalDamage)));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetContractState();
        var state2 = GetBindingTermsState();
        var state3 = GetMissileState();
        var state4 = GetOnslaughtState();

        state1.FollowUpState = state4;
        state2.FollowUpState = state4;
        state3.FollowUpState = state2;
        state4.FollowUpState = state3;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        
        return new MonsterMoveStateMachine(states, state1);
    }

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetSafeguardState();
        var state2 = GetMissileState();
        var state3 = GetOnslaughtState();

        state1.FollowUpState = state3;
        state2.FollowUpState = state1;
        state3.FollowUpState = state2;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        
        return new MonsterMoveStateMachine(states, state3);
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
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<MagicMissile>();
        card1.SetDamage(MissileDamage);
        card1.SetRepeat(MissileHits);
        var card2 = CreateCardForIntent<MagicOnslaught>();
        card2.SetDamage(OnslaughtDamage);
        card2.DynamicVars["Increase"].BaseValue = OnslaughtDamageIncrease;
        var card3 = CreateCardForIntent<MagicSafeguard>();
        card3.SetBlock(BlockAmt);
        card3.SetStrength(StrengthAmount);
        var card4 = CreateCardForIntent<Contract>();
        var card5 = CreateCardForIntent<BindingTerms>();
        card5.SetCards(StatusAmt);
        return new Dictionary<string, CardModel>()
        {
            {MISSILE, card1},
            {ONSLAUGHT, card2},
            {SAFEGUARD, card3},
            {CONTRACT, card4},
            {BINDING_TERMS, card5},
        };
    }

    private async Task Safeguard(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Missile(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < MissileHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(MissileDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task BindingTerms(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        await CardPileCmd.AddToCombatAndPreview<Void>(targets, PileType.Discard, StatusAmt, null);
        await ResetIdle(1.0f);
    }
    
    private async Task Contract(IReadOnlyList<Creature> targets)
    {
        await SpecialAnimation(targets);
        List<Task> taskList = new List<Task>();
        foreach (Creature target in targets)
        {
            taskList.Add(ChooseContract(target));
        }
        await Task.WhenAll(taskList);
        if (!CombatState.IsLiveCombat())
            return;
        await ResetIdle(1.0f);
    }
    
    public async Task ChooseContract(Creature target)
    {
        if (target.IsDead)
        {
            return;
        }
        if (target.Player != null)
        {
            CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), ContractSet.Select((Func<IChoosable, CardModel>) (c =>
            {
                if (target.Player != null)
                {
                    return CombatState.CreateCard((CardModel) c, target.Player);
                }
                return null;
            })).ToList(), target.Player);
            if (cardModel == null)
            {
                return;
            }
            await ((IChoosable) cardModel).OnChosen();
        }
    }
    
    private async Task Onslaught(IReadOnlyList<Creature> targets)
    {
        await BluntAnimation(targets);
        await DamageCmd.Attack(OnslaughtTotalDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
        CurrentDamageIncrease += OnslaughtDamageIncrease;
        var card = MoveToCardMap[ONSLAUGHT];
        if (card is MagicOnslaught onslaught)
        {
            onslaught.SetDamage(OnslaughtTotalDamage);
        }
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Hokma hokma)
        {
            if (hokma.Creature.IsAlive)
            {
                await hokma.OnBossDeath();
            }
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.PlutoVert, targets);
    }

    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.PlutoStab, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.PlutoHori, targets);
    }
    
    private async Task SpecialAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special", Sfx.PlutoContract, targets);
    }
    
    private async Task BlockAnimation()
    {
        await AnimationAction("Block", Sfx.PlutoGuard);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Slash", "Block", "Special", "Special2"], controller);
    }
    
    public interface IChoosable
    {
        Task OnChosen();
    }
}