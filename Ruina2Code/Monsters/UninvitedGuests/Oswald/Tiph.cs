using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards;
using Ruina2.Ruina2Code.Cards.EnemyCards.Tiph;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;
using Brainwash = Ruina2.Ruina2Code.Afflictions.Brainwash;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

public sealed class Tiph : AbstractAllyCardMonster
{
    public override int MinInitialHp => 300;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "TiphIcon.png".UIImagePath();
    private bool talked = false;

    private int KickDamage => 10;
    private int ConfrontationDamage => 16;
    private int TrigramDamage => 12;
    private int TrigramHits => 2;
    private int StrengthAmt => 2;
    private int BlockAmt => 10;
    public static int GEON = 50;
    public static int GON = 50;
    public static int RI = 1;
    protected override string VisualsPath => "Tiph/tiph.tscn".MonsterImagePath();

    private const string AUGURY_KICK = "AUGURY_KICK";
    private const string CONFRONTATION = "CONFRONTATION";
    private const string TRIGRAM = "TRIGRAM";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Oswald>();
        await PowerCmd.Apply<Geon>(new ThrowingPlayerChoiceContext(), Creature, GEON, Creature,  null);
    }

    private MoveState GetAuguryKickState()
    {
        return new MoveState(AUGURY_KICK, AuguryKick, new RuinaSingleAttackIntent(KickDamage), new BuffIntent());
    }

    private MoveState GetConfrontationState()
    {
        return new MoveState(CONFRONTATION, Confrontation, new RuinaSingleAttackIntent(ConfrontationDamage), new DefendIntent());
    }
    
    private MoveState GetTrigramState()
    {
        return new MoveState(TRIGRAM, Trigram, new RuinaMultiAttackIntent(TrigramDamage, TrigramHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetAuguryKickState();
        var state2 = GetConfrontationState();
        var state3 = GetTrigramState();

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
        var card1 = CreateCardForIntent<Trigram>();
        card1.SetDamage(TrigramDamage);
        card1.SetRepeat(TrigramHits);
        card1.DynamicVars.Damage.EnchantedValue = TrigramDamage + 2;
        card1.DynamicVars.Damage.PreviewValue = TrigramDamage + 4;
        var test = card1.DynamicVars.Damage.ToHighlightedString(false);
        MainFile.Logger.Info("test string: "+test);
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
            TalkCmd.Play(L10NMonsterLookup("RUINA2-TIPH.response"), Creature, VfxColor.Gold);
            talked = true;
        }
    }

    private async Task AuguryKick(IReadOnlyList<Creature> targets)
    {
        Talk();
        await BluntAnimation(targets);
        await DamageCmd.Attack(KickDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmt, Creature,  null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, StrengthAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Confrontation(IReadOnlyList<Creature> targets)
    {
        Talk();
        await PierceAnimation(targets);
        await DamageCmd.Attack(ConfrontationDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        foreach (var player in CombatState.PlayerCreatures)
        {
            await CreatureCmd.GainBlock(player, BlockAmt, ValueProp.Move, null);
        }
        await ResetIdle();
    }
    
    private async Task Trigram(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < TrigramHits; i++)
        {
            if (i % 2 == 0) {
                await Special1Animation(targets);
                await WaitAnimation(1.0f);
                await Special2Animation(targets);
            } else {
                await Special1AnimationNoSound(targets);
                await WaitAnimation();
                await Special4Animation(targets);
            }
            await DamageCmd.Attack(TrigramDamage)
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
        TalkCmd.Play(L10NMonsterLookup("RUINA2-TIPH.victory"), Creature, VfxColor.Gold);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }
    
    private bool SwitchedOffAllyProtection = false;
    public bool IsTargetableByPlayersMutable = true;
    
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (!(cardPlay.Card.Affliction is Brainwash) && IsTargetableByPlayers)
        {
            IsTargetableByPlayers = false;
            SwitchedOffAllyProtection = true;
            IsTargetableByPlayersMutable = false;
        }
        return Task.CompletedTask;
    }
    
    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (SwitchedOffAllyProtection)
        {
            IsTargetableByPlayers = true;
            SwitchedOffAllyProtection = false;
            IsTargetableByPlayersMutable = true;
        }
        return Task.CompletedTask;
    }

    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.HanaBlunt, targets);
    }
    
    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.HanaStab, targets);
    }
    
    private async Task Special1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special1", Sfx.HanaStrongCharge, targets);
    }
    
    private async Task Special1AnimationNoSound(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special1", null, targets);
    }
    
    private async Task Special2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special2", Sfx.HanaStrongStart, targets);
    }
    
    private async Task Special4Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special4", Sfx.HanaStrongFin, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Special1", "Special2", "Special4"], controller);
    }
}