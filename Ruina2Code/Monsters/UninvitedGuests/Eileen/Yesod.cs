using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Tiph;
using Ruina2.Ruina2Code.Cards.EnemyCards.Yesod;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Eileen;

public sealed class Yesod : AbstractAllyCardMonster
{
    public override int MinInitialHp => 160;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "YesodIcon.png".UIImagePath();
    private bool talked = false;

    private int BulletsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int BulletHits => 3;
    private int BlockAmt => 14;
    private int Energy => 1;
    private int Draw => 1;
    private decimal InitialDamageBonus = 100M;
    private decimal DamageIncrease = 50M;
    protected override string VisualsPath => "Yesod/yesod.tscn".MonsterImagePath();

    private const string FLOODING_BULLETS = "FLOODING_BULLETS";
    private const string RELOAD = "RELOAD";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Eileen>();
        MassAttackHitsPlayer = true;
        var power = await PowerCmd.Apply<DarkBargain>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature,  null);
        if (power != null)
        {
            power.DynamicVars["DamageBonus"].BaseValue = InitialDamageBonus;
            power.DynamicVars["Increase"].BaseValue = (DamageIncrease / CombatState.PlayerCreatures.Count);
        }
    }

    private MoveState GetFloodingBulletsState()
    {
        return new MoveState(FLOODING_BULLETS, FloodingBullets, new RuinaMultiMassAttackIntent(BulletsDamage, BulletHits));
    }

    private MoveState GetReloadState()
    {
        return new MoveState(RELOAD, Reload, new RuinaDefendIntent(), new RuinaBuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetFloodingBulletsState();
        var state2 = GetReloadState();

        state1.FollowUpState = state2;
        state2.FollowUpState = state1;

        states.Add(state1);
        states.Add(state2);
        
        return new MonsterMoveStateMachine(states, state1);
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<FloodingBullets>();
        card1.SetDamage(BulletsDamage);
        card1.SetRepeat(BulletHits);
        var card2 = CreateCardForIntent<Reload>();
        card2.SetBlock(BlockAmt);
        card2.SetEnergy(Energy);
        card2.SetCards(Draw);
        return new Dictionary<string, CardModel>()
        {
            {FLOODING_BULLETS, card1},
            {RELOAD, card2}
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-YESOD.response"), Creature, VfxColor.Purple);
            talked = true;
        }
    }
    
    public override IReadOnlyList<Creature> AdditionalMassAttackTargets()
    {
        var newList = new List<Creature>();
        foreach (var hittableEnemy in CombatState.HittableEnemies)
        {
            if (hittableEnemy != Creature)
            {
                newList.Add(hittableEnemy);
            }
        }
        return newList;
    }

    private async Task FloodingBullets(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < BulletHits; i++)
        {
            IsMassAttacking = true;
            await RangedAnimation(targets);
            await WaitAnimation(1.0f);
            await DamageCmd.Attack(BulletsDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
            await ResetIdle();
            await WaitAnimation();
        }
    }
    
    private async Task Reload(IReadOnlyList<Creature> targets)
    {
        Talk();
        await SpecialAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        await PowerCmd.Apply<EnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, Energy, Creature,  null);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, Draw, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-YESOD.victory"), Creature, VfxColor.Purple);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }
    
    private async Task RangedAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", Sfx.BulletFinalShot, targets);
    }
    
    private async Task SpecialAnimation()
    {
        await AnimationAction("Special", Sfx.BulletFlame);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Ranged", "Special"], controller);
    }
}