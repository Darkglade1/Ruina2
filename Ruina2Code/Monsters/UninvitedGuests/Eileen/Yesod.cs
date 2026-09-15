using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
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
using Ruina2.Ruina2Code.Cards.EnemyCards.Tiph;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;
using Brainwash = Ruina2.Ruina2Code.Afflictions.Brainwash;

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
    private float InitialDamageBonus = 2.0f;
    private float DamageIncrease = 0.5f;
    protected override string VisualsPath => "Yesod/yesod.tscn".MonsterImagePath();

    private const string FLOODING_BULLETS = "FLOODING_BULLETS";
    private const string RELOAD = "RELOAD";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Eileen>();
        MassAttackHitsPlayer = true;
        //await PowerCmd.Apply<Geon>(new ThrowingPlayerChoiceContext(), Creature, GEON, Creature,  null);
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

        state1.FollowUpState = state1;
        state2.FollowUpState = state2;

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
        var card1 = CreateCardForIntent<Trigram>();
        card1.SetDamage(TrigramDamage);
        card1.SetRepeat(TrigramHits);
        var card2 = CreateCardForIntent<Confrontation>();
        card2.SetDamage(ConfrontationDamage);
        card2.SetBlock(BlockAmt);
        var card3 = CreateCardForIntent<AuguryKick>();
        card3.SetDamage(KickDamage);
        card3.SetStrength(StrengthAmt);
        return new Dictionary<string, CardModel>()
        {
            {TRIGRAM, card1},
            {CONFRONTATION, card2},
            {AUGURY_KICK, card3},
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