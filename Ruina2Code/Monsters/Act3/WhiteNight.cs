using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Nodes;
using Ruina2.Ruina2Code.Patches;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3;

public sealed class WhiteNight : AbstractRuinaMonster
{
    public override int MinInitialHp => 666;
    public override int MaxInitialHp => MinInitialHp;
    
    private int RiseDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 50, 45);
    private int BeholdDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 28, 25);
    private int RegenAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 6, 0);
    private int RitualGain => 1;
    private int HealAmt => 15;
    private int BlockAmt => 25;
    private int AdventCards => 12;
    private int StatusAmt =>  AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    protected override string VisualsPath => "WhiteNight/white_night.tscn".MonsterImagePath();

    private const string PRAYER = "PRAYER";
    private const string RISE_AND_SERVE = "LUMBER";
    private const string BENEDICTION = "BENEDICTION";
    private const string SALVATION = "SALVATION";
    private const string BEHOLD = "BEHOLD";

    private bool awake;
    private bool shouldBuff = true;
    public int ApostleUpgradeCount = 0;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<Advent>(new ThrowingPlayerChoiceContext(), Creature, AdventCards * CombatState.Players.Count, Creature, null);
        if (RegenAmt > 0)
        {
            await PowerCmd.Apply<MonsterRegen>(new ThrowingPlayerChoiceContext(), Creature, Creature.ScaleHpForMultiplayer(RegenAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature, null);
        }
    }

    private MoveState GetPrayerState()
    {
        return new MoveState(PRAYER, Prayer, new BuffIntent());
    }

    private MoveState GetRiseAndServeState()
    {
        return new MoveState(RISE_AND_SERVE, RiseAndServe, new SingleAttackIntent(RiseDamage), new CardDebuffIntent());
    }

    private MoveState GetBenedictionState()
    {
        return new MoveState(BENEDICTION, Benediction, new DefendIntent(), new HealIntent(), new BuffIntent());
    }
    
    private MoveState GetSalvationState()
    {
        return new MoveState(SALVATION, Salvation, new StatusIntent(StatusAmt), new BuffIntent());
    }
    
    private MoveState GetBeholdState()
    {
        return new MoveState(BEHOLD, Behold, new SingleAttackIntent(BeholdDamage));
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetPrayerState();
        var state2 = GetRiseAndServeState();
        var state3 = GetBenedictionState();
        var state4 = GetSalvationState();
        var state5 = GetBeholdState();
        
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
        if (!awake)
        {
            return PRAYER;
        } else if (LastMove(stateMachine, PRAYER))
        {
            return RISE_AND_SERVE;
        } else if (LastMove(stateMachine, RISE_AND_SERVE) || LastMove(stateMachine, BEHOLD))
        {
            if (shouldBuff)
            {
                shouldBuff = !shouldBuff;
                return BENEDICTION;
            }
            else
            {
                shouldBuff = !shouldBuff;
                return SALVATION;
            }
            
        } else 
        {
            return BEHOLD;
        }
    }
    
    private async Task Prayer(IReadOnlyList<Creature> targets)
    {
        await BlessAnimation();
        await PowerCmd.Apply<RitualPower>(new ThrowingPlayerChoiceContext(), Creature, RitualGain, Creature,  null);
        await WaitAnimation(1.0f);
    }
    
    private async Task RiseAndServe(IReadOnlyList<Creature> targets)
    {
        await ShockwaveAnimation();
        await DamageCmd.Attack(RiseDamage)
            .FromMonster(this)
            .Execute(null);
        await WaitAnimation(1.0f);
        await RiseAndServeFullScreenEffect();
        foreach (var target in targets)
        {
            if (target.Player != null)
            {
                List<CardModel> allApostles = new List<CardModel>();
                if (target.Player?.PlayerCombatState != null)
                {
                    foreach (CardModel card in target.Player.PlayerCombatState.AllCards)
                    {
                        if (card.Affliction is Afflictions.Apostle)
                        {
                            allApostles.Add(card);
                        }
                    }
                }
                foreach (CardModel apostle in allApostles)
                {
                    await CardCmd.TransformTo<Apostle>(apostle, CardPreviewStyle.MessyLayout);
                }
            }
        }
        await WaitAnimation(1.0f);
    }
    
    private async Task Benediction(IReadOnlyList<Creature> targets)
    {
        await BlessAnimation();
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);   
        await CreatureCmd.Heal(Creature, Creature.ScaleHpForMultiplayer(HealAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex));
        int strAmt = 3;
        int ritualAmount = Creature.GetPowerAmount<RitualPower>();
        if (ritualAmount > 0)
        {
            strAmt = ritualAmount;
        }
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, strAmt, Creature, null);
        await WaitAnimation(1.0f);
    }
    
    private async Task Salvation(IReadOnlyList<Creature> targets)
    {
        await BlessAnimation();
        foreach (Creature target in targets)
        {
            if (target.Player?.PlayerCombatState != null)
            {
                foreach (CardModel allCard in target.Player.PlayerCombatState.AllCards)
                {
                    if (allCard is Apostle apostle)
                        apostle.FakeUpgrade();
                }
            }
        }
        ApostleUpgradeCount++;
        await CardPileCmd.AddToCombatAndPreview<Apostle>(CombatState.PlayerCreatures, PileType.Draw, StatusAmt, null, CardPilePosition.Random);
        await WaitAnimation(1.0f);
    }
    
    private async Task Behold(IReadOnlyList<Creature> targets)
    {
        await ShockwaveAnimation();
        await DamageCmd.Attack(BeholdDamage)
            .FromMonster(this)
            .Execute(null); 
        await WaitAnimation(1.0f);
    }

    public async Task Awaken()
    {
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (creatureNode != null)
        {
            if (creatureNode.Visuals is Ruina2NCreatureVisuals visuals)
            {
                var fullScreenEffect = FullScreenImageEffect.Create("BlackScreen.png".VfxImagePath(), 0.3f, 1.2f);
                Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
                vfxContainer?.AddChildSafely(fullScreenEffect);
                visuals.SetSpineIdleAnimation();
                Sfx.WhiteNightAppear.Play();
                MusicPatches.RuinaActMusicPatches.OnWhiteNightAwakened();
                await PowerCmd.Remove<Advent>(Creature);
                awake = true;
                await WaitAnimation(1.0f);
            }
        }
    }
    
    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (!(card is Apostle apostle))
            return Task.CompletedTask;
        MatchApostleToUpgradeCount(apostle);
        return Task.CompletedTask;
    }

    public void MatchApostleToUpgradeCount(Apostle apostle)
    {
        for (int index = 0; index < ApostleUpgradeCount; ++index)
            apostle.FakeUpgrade();
    }
    
    private async Task BlessAnimation()
    {
        await SoundAnimation(Sfx.ProphetBless, null);
    }

    private async Task RiseAndServeFullScreenEffect()
    {
        var fullScreenEffect = FullScreenImageEffect.Create("Apostles.png".VfxImagePath(), 1.0f, 1.0f);
        Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        vfxContainer?.AddChildSafely(fullScreenEffect);
        Sfx.WhiteNightSummon.Play();
        await WaitAnimation(2.0f);
    }
    
    private async Task ShockwaveAnimation()
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(Creature);
        if (node != null)
        {
            var shockwaveEffect = ShockwaveEffect.Create(node.VfxSpawnPosition);
            Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
            vfxContainer?.AddChildSafely(shockwaveEffect);
            await WaitAnimation(2.0f);
        }
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle"], controller);
    }
}