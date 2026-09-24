using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Roland;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Argalia;

public sealed class Roland : AbstractAllyCardMonster
{
    public override int MinInitialHp => 500;
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 1;
    public override string TargetTexturePath => "RolandIcon.png".UIImagePath();
    private bool talked = false;

    private int CrystalDamage => 22;
    private int CrystalHits => 2;
    private int CrystalBlock => 33;
    private int WheelsDamage => 45;
    private int WheelsStrDown => 4;
    private int DurandalDamage => 15;
    private int DurandalHits => 2;
    private int DurandalStrength => 4;
    private int AllasDamage => 32;
    private int AllasDebuff => 2;
    private int GunDamage => 16;
    private int GunHits => 3;
    private int MookDamage => 24;
    private int MookDebuff => 2;
    private int OldBoyDamage => 12;
    private int OldBoyBlock => 20;
    private int RangaDamage => 9;
    private int RangaHits => 3;
    private int RangaBleed => 8;
    private int MaceDamage => 14;
    private int MaceHits => 2;
    private int FuriosoDamage => 25;
    private int FuriosoHits => 16;
    private int PowerStrength => 2;

    private List<String> movePool = new List<string>();
  
    protected override string VisualsPath => "Roland/roland.tscn".MonsterImagePath();

    private const string CRYSTAL = "CRYSTAL";
    private const string WHEELS = "WHEELS";
    private const string DURANDAL = "DURANDAL";
    private const string ALLAS = "ALLAS";
    private const string GUN = "GUN";
    private const string MOOK = "MOOK";
    private const string OLD_BOY = "OLD_BOY";
    private const string RANGA = "RANGA";
    private const string MACE = "MACE";
    public static string FURIOSO = "FURIOSO";

    public override async Task AfterAddedToRoom()
    { 
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Argalia>();
        await PowerCmd.Apply<BlackSilence>(new ThrowingPlayerChoiceContext(), Creature, PowerStrength, Creature,  null);
        PopulateMovePool();
    }

    private MoveState GetCrystalState()
    {
        return new MoveState(CRYSTAL, Crystal, new RuinaMultiAttackIntent(CrystalDamage, CrystalHits), new RuinaDefendIntent());
    }

    private MoveState GetWheelsState()
    {
        return new MoveState(WHEELS, Wheels, new RuinaSingleAttackIntent(WheelsDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetDurandalState()
    {
        return new MoveState(DURANDAL, Durandal, new RuinaMultiAttackIntent(DurandalDamage, DurandalHits), new RuinaBuffIntent());
    }
    
    private MoveState GetAllasState()
    {
        return new MoveState(ALLAS, Allas, new RuinaSingleAttackIntent(AllasDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetGunState()
    {
        return new MoveState(GUN, Gun, new RuinaMultiAttackIntent(GunDamage, GunHits));
    }
    
    private MoveState GetMookState()
    {
        return new MoveState(MOOK, Mook, new RuinaSingleAttackIntent(MookDamage), new RuinaDebuffIntent());
    }
    
    private MoveState GetOldBoyState()
    {
        return new MoveState(OLD_BOY, OldBoy, new RuinaSingleAttackIntent(OldBoyDamage), new RuinaDefendIntent());
    }
    
    private MoveState GetRangaState()
    {
        return new MoveState(RANGA, Ranga, new RuinaMultiAttackIntent(RangaDamage, RangaHits), new RuinaDebuffIntent());
    }
    
    private MoveState GetMaceState()
    {
        return new MoveState(MACE, Mace, new RuinaMultiAttackIntent(MaceDamage, MaceHits));
    }
    
    private MoveState GetFuriosoState()
    {
        return new MoveState(FURIOSO, Furioso, new RuinaMultiAttackIntent(FuriosoDamage, FuriosoHits));
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetCrystalState();
        var state2 = GetWheelsState();
        var state3 = GetDurandalState();
        var state4 = GetAllasState();
        var state5 = GetGunState();
        var state6 = GetMookState();
        var state7 = GetOldBoyState();
        var state8 = GetRangaState();
        var state9 = GetMaceState();
        var state10 = GetFuriosoState();
        
        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 0);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;
        state4.FollowUpState = moveBranch;
        state5.FollowUpState = moveBranch;
        state6.FollowUpState = moveBranch;
        state7.FollowUpState = moveBranch;
        state8.FollowUpState = moveBranch;
        state9.FollowUpState = moveBranch;
        state10.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(state4);
        states.Add(state5);
        states.Add(state6);
        states.Add(state7);
        states.Add(state8);
        states.Add(state9);
        states.Add(state10);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }

    private void PopulateMovePool()
    {
        movePool.Add(CRYSTAL);
        movePool.Add(WHEELS);
        movePool.Add(DURANDAL);
        movePool.Add(ALLAS);
        movePool.Add(GUN);
        movePool.Add(MOOK);
        movePool.Add(OLD_BOY);
        movePool.Add(RANGA);
        movePool.Add(MACE);
    }

    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (movePool.Count == 0)
        {
            PopulateMovePool();
            return FURIOSO;
        }
        var nextMove = movePool[rng.NextInt(movePool.Count)];
        movePool.Remove(nextMove);
        return nextMove;
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
        var card1 = CreateCardForIntent<Atelier>();
        card1.SetDamage(GunDamage);
        card1.SetRepeat(GunHits);
        var card2 = CreateCardForIntent<Allas>();
        card2.SetDamage(AllasDamage);
        card2.SetWeak(AllasDebuff);
        var card3 = CreateCardForIntent<Ranga>();
        card3.SetDamage(RangaDamage);
        card3.SetRepeat(RangaHits);
        card3.DynamicVars["BleedEnemy"].BaseValue = RangaBleed;
        var card4 = CreateCardForIntent<Crystal>();
        card4.SetDamage(CrystalDamage);
        card4.SetRepeat(CrystalHits);
        card4.SetBlock(CrystalBlock);
        var card5 = CreateCardForIntent<Durandal>();
        card5.SetDamage(DurandalDamage);
        card5.SetRepeat(DurandalHits);
        card5.SetStrength(DurandalStrength);
        var card6 = CreateCardForIntent<Furioso>();
        card6.SetDamage(FuriosoDamage);
        card6.SetRepeat(FuriosoHits);
        var card7 = CreateCardForIntent<Mook>();
        card7.SetDamage(MookDamage);
        card7.SetVulnerable(MookDebuff);
        var card8 = CreateCardForIntent<OldBoys>();
        card8.SetDamage(OldBoyDamage);
        card8.SetBlock(OldBoyBlock);
        var card9 = CreateCardForIntent<Wheels>();
        card9.SetDamage(WheelsDamage);
        card9.SetStrength(WheelsStrDown);
        var card10 = CreateCardForIntent<Zelkova>();
        card10.SetDamage(MaceDamage);
        card10.SetRepeat(MaceHits);
        return new Dictionary<string, CardModel>()
        {
            {GUN, card1},
            {ALLAS, card2},
            {RANGA, card3},
            {CRYSTAL, card4},
            {DURANDAL, card5},
            {FURIOSO, card6},
            {MOOK, card7},
            {OLD_BOY, card8},
            {WHEELS, card9},
            {MACE, card10},
        };
    }

    private void Talk()
    {
        if (!talked)
        {
            TalkCmd.Play(L10NMonsterLookup("RUINA2-ROLAND.response"), Creature, VfxColor.Black);
            talked = true;
        }
    }

    private async Task Crystal(IReadOnlyList<Creature> targets)
    {
        Talk();
        await CreatureCmd.GainBlock(Creature, CrystalBlock, ValueProp.Move, null);
        for (int i = 0; i < CrystalHits; i++)
        {
            await SlashAnimation(targets);
            await DamageCmd.Attack(CrystalDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle(0.25f);
            await WaitAnimation(0.25f);
        }
    }
    
    private async Task Wheels(IReadOnlyList<Creature> targets)
    {
        Talk();
        await WheelsAnimation(targets);
        await DamageCmd.Attack(WheelsDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -WheelsStrDown, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Durandal(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < DurandalHits; i++)
        {
            if (i % 2 == 0)
            {
                await Sword2Animation(targets);
            } 
            else
            {
                await Sword3Animation(targets);
            }
            await DamageCmd.Attack(DurandalDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, DurandalStrength, Creature,  null);
    }
    
    private async Task Allas(IReadOnlyList<Creature> targets)
    {
        Talk();
        await LanceAnimation(targets);
        await DamageCmd.Attack(AllasDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, AllasDebuff, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Gun(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < GunHits; i++)
        {
            if (i == 0) {
                await Gun1Animation(targets);
            } else if (i == 1) {
                await Gun2Animation(targets);
            } else {
                await Gun3Animation(targets);
            }
            await DamageCmd.Attack(GunDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Mook(IReadOnlyList<Creature> targets)
    {
        Talk();
        await Mook1Animation(targets);
        await WaitAnimation(0.25f);
        await Mook2Animation(targets);
        await DamageCmd.Attack(MookDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ApplyPowerAndSkipNextDurationTickIfNotPresent<VulnerablePower>(targets, MookDebuff);
        await ResetIdle(1.0f);
    }
    
    private async Task OldBoy(IReadOnlyList<Creature> targets)
    {
        Talk();
        await HammerAnimation(targets);
        await CreatureCmd.GainBlock(Creature, OldBoyBlock, ValueProp.Move, null);
        await DamageCmd.Attack(OldBoyDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await ResetIdle();
    }
    
    private async Task Ranga(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < RangaHits; i++)
        {
            if (i == 0) {
                await Claw1Animation(targets);
            } else if (i == 1) {
                await Claw2Animation(targets);
            } else {
                await Claw3Animation(targets);
            }
            await DamageCmd.Attack(RangaDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
        await PowerCmd.Apply<BleedEnemy>(new ThrowingPlayerChoiceContext(), targets, RangaBleed , Creature, null);
    }
    
    private async Task Mace(IReadOnlyList<Creature> targets)
    {
        Talk();
        for (int i = 0; i < MaceHits; i++)
        {
            if (i % 2 == 0)
            {
                await Mace1Animation(targets);
            } 
            else
            {
                await Mace2Animation(targets);
            }
            await DamageCmd.Attack(MaceDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Furioso(IReadOnlyList<Creature> targets)
    {
        Talk();
        if (NCombatRoom.Instance != null)
        {
            NCreature? creatureNode = NCombatRoom.Instance.GetCreatureNode(Creature);
            NCreature? targetCreatureNode = NCombatRoom.Instance.GetCreatureNode(targets[0]);

            if (creatureNode != null && targetCreatureNode != null)
            {
                float initialX = creatureNode.GlobalPosition.X;
                float targetBehind = targetCreatureNode.GlobalPosition.X + 150.0f;
                float targetFront = targetCreatureNode.GlobalPosition.X - 200.0f;
                float attackInterval = 0.35f;

                await Gun1Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await Gun2Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await MoveAnimation(targetBehind, targets);
                await LanceAnimation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await FlipAnimation(true, targets);
                await HammerAnimation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await MoveAnimation(targetFront, targets);
                await Claw3Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await FlipAnimation(false, targets);
                await Mook1Animation(targets);
                await WaitAnimation(0.15f, targets);
                await Mook2Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await MoveAnimation(targetBehind, targets);
                await Claw1Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await MoveAnimation(targetFront, targets);
                await FlipAnimation(true, targets);
                await Claw2Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await FlipAnimation(false, targets);
                await Mace1Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await Mace2Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await WheelsAnimation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await SlashAnimation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await Gun3Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await Sword1Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await Sword2Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await WaitAnimation(attackInterval, targets);
                await Sword3Animation(targets);
                await DamageCmd.Attack(FuriosoDamage).FromMonsterCreature(this).TargetingCreatures(targets, CombatState).Execute(null);
                await ResetIdle();
                await MoveAnimation(initialX, null);
                await FlipAnimation(false, null);
            }
        }
    }
    
    public async Task OnBossDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-NETZACH.victory"), Creature, VfxColor.Gold);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }

    private async Task HammerAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Hammer", Sfx.RolandAxe, targets);
    }
    
    private async Task LanceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Lance", Sfx.RolandAxe, targets);
    }
    
    private async Task Gun1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Gun1", Sfx.RolandRevolver, targets);
    }
    
    private async Task Gun2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Gun2", Sfx.RolandRevolver, targets);
    }
    
    private async Task Gun3Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Gun3", Sfx.RolandShotgun, targets);
    }
    
    private async Task Mook1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Mook1", Sfx.RolandLongSwordStart, targets);
    }
    
    private async Task Mook2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Mook2", Sfx.RolandLongSwordAtk, targets);
        await AnimationAction("Mook2", Sfx.RolandLongSwordFin, targets);
    }
    
    private async Task Claw1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Claw1", Sfx.SwordStab, targets);
    }
    
    private async Task Claw2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Claw2", Sfx.SwordStab, targets);
    }
    
    private async Task Claw3Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Claw3", Sfx.RolandShortSword, targets);
    }
    
    private async Task Mace1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Mace1", Sfx.BluntVert, targets);
    }
    
    private async Task Mace2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Mace2", Sfx.BluntVert, targets);
    }
    
    private async Task WheelsAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Wheels", Sfx.RolandGreatSword, targets);
    }
    
    private async Task Sword1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Sword1", Sfx.RolandDuralandalDown, targets);
    }
    
    private async Task Sword2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Sword2", Sfx.RolandDuralandalUp, targets);
    }
    
    private async Task Sword3Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Sword3", Sfx.RolandDuralandalStrong, targets);
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.RolandDualSword, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Gun1", "Gun2", "Gun3", "Hammer", "Claw1", "Claw2", "Claw3", "Lance", "Mace1", "Mace2", "Mook1", "Mook2", "Slash", "Sword1", "Sword2", "Sword3", "Wheels"], controller);
    }
}