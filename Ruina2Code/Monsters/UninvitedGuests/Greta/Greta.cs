using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Cards.EnemyCards.Greta;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Powers;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Greta;

public sealed class Greta : AbstractCardMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 820, 750);
    public override int MaxInitialHp => MinInitialHp;
    public override int NumIntents => 3;

    private int BreakEggDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 23, 21);
    private int MinceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);
    private int MinceHits => 2;
    private int TrialDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int ParalysisAmt => 2;
    private int BleedAmt => 4;
    private int AllyDebuffAmt => 3;
    private int BlockAmt => 20;
    private int DamageReduction => 50;
    private int PowerTurns => 3;
    private int Artifact => 2;

    protected override string VisualsPath => "Greta/greta.tscn".MonsterImagePath();

    private const string BREAK_EGG = "BREAK_EGG";
    private const string SLAP = "SLAP";
    private const string MINCE  = "MINCE";
    private const string SEASON = "SEASON";
    private const string TRIAL = "TRIAL";
    private const string SACK = "SACK";

    private Creature? meat;
    
    private static readonly Func<CardModel, bool>[] _stealPriorities = new Func<CardModel, bool>[4]
    {
        c => !(c.Enchantment is Imbued) && c.Rarity == CardRarity.Rare,
        c =>
        {
            bool flag1 = !(c.Enchantment is Imbued);
            if (flag1)
            {
                bool flag2;
                switch (c.Rarity)
                {
                    case CardRarity.Common:
                    case CardRarity.Uncommon:
                    case CardRarity.Event:
                        flag2 = true;
                        break;
                    default:
                        flag2 = false;
                        break;
                }
                flag1 = flag2;
            }
            return flag1;
        },
        c =>
        {
            bool flag3 = !(c.Enchantment is Imbued);
            if (flag3)
            {
                bool flag4;
                switch (c.Rarity)
                {
                    case CardRarity.Basic:
                    case CardRarity.Quest:
                        flag4 = true;
                        break;
                    default:
                        flag4 = false;
                        break;
                }
                flag3 = flag4;
            }
            return flag3;
        },
        c => c.Rarity == CardRarity.Ancient || c.Enchantment is Imbued
    };

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        OtherSideTargetMonster = FindTarget<Hod>();
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, Artifact, Creature, null);
        var sharkskin = await PowerCmd.Apply<Sharkskin>(new ThrowingPlayerChoiceContext(), Creature, DamageReduction, Creature, null);
        if (sharkskin != null)
        {
            sharkskin.DynamicVars["Turns"].BaseValue = PowerTurns;
            sharkskin.DynamicVars["ArtifactPower"].BaseValue = Artifact;
        }
        TalkCmd.Play(L10NMonsterLookup("RUINA2-GRETA.talk"), Creature, VfxColor.Gold);
    }

    private MoveState GetBreakEggState()
    {
        return new MoveState(BREAK_EGG, BreakEgg, new RuinaSingleAttackIntent(BreakEggDamage), new RuinaDebuffIntent());
    }

    private MoveState GetMinceState()
    {
        return new MoveState(MINCE, Mince, new RuinaMultiAttackIntent(MinceDamage, MinceHits));
    }

    private MoveState GetSlapState()
    {
        return new MoveState(SLAP, Slap, new DefendIntent(), new RuinaDebuffIntent());
    }
    
    private MoveState GetSeasonState()
    {
        return new MoveState(SEASON, Season, new RuinaDebuffIntent());
    }
    
    private MoveState GetSackState()
    {
        return new MoveState(SACK, Sack, new SummonIntent(), new CardDebuffIntent());
    }
    
    private MoveState GetTrialState()
    {
        return new MoveState(TRIAL, Trial, new RuinaSingleAttackIntent(TrialDamage), new HealIntent(), new BuffIntent());
    }

    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetBreakEggState();
        var state2 = GetMinceState();
        var state3 = GetSlapState();
        
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

    private MonsterMoveStateMachine GenerateIntent2StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetMinceState();
        var state2 = GetSlapState();
        var state3 = GetSeasonState();

        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 1);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;
        state3.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(state3);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private MonsterMoveStateMachine GenerateIntent3StateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetTrialState();
        var state2 = GetSackState();

        var moveBranch = new ConditionalBranchState("MOVE_BRANCH", SelectNextMove, 2);

        state1.FollowUpState = moveBranch;
        state2.FollowUpState = moveBranch;

        states.Add(state1);
        states.Add(state2);
        states.Add(moveBranch);
        
        return new MonsterMoveStateMachine(states, moveBranch);
    }
    
    private string SelectNextMove(Creature owner, Rng rng, MonsterMoveStateMachine stateMachine, int intentNum)
    {
        if (intentNum == 0)
        {
            if (stateMachine.StateLog.Count >= 3)
            {
                stateMachine.StateLog.Clear();
            }
            List<string> possibilities = new List<string>();
            if (!LastMove(stateMachine, BREAK_EGG) && !LastMoveBefore(stateMachine, BREAK_EGG)) {
                possibilities.Add(BREAK_EGG);
            }
            if (!LastMove(stateMachine, MINCE) && !LastMoveBefore(stateMachine, MINCE)) {
                possibilities.Add(MINCE);
            }
            if (!LastMove(stateMachine, SLAP) && !LastMoveBefore(stateMachine, SLAP)) {
                possibilities.Add(SLAP);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }

        if (intentNum == 1)
        {
            List<string> possibilities = new List<string>();
            if (!LastTwoMoves(stateMachine, MINCE)) {
                possibilities.Add(MINCE);
            }
            if (!LastMove(stateMachine, SEASON) && !LastMoveBefore(stateMachine, SEASON)) {
                possibilities.Add(SEASON);
            }
            if (!LastMove(stateMachine, SLAP) && !LastMoveBefore(stateMachine, SLAP)) {
                possibilities.Add(SLAP);
            }
            return possibilities[rng.NextInt(possibilities.Count)];
        }

        if (meat == null || meat.IsDead)
        {
            return SACK;
        }
        return TRIAL;
    }

    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine(), GenerateIntent2StateMachine(), GenerateIntent3StateMachine()];
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
        if (intentNum == 2 && NextMoves.Count >= 3 && meat != null)
        {
            if (NextMoves[intentNum].Id == TRIAL)
            {
                return meat;
            }
        }
        return CombatState.PlayerCreatures[0];
    }
    
    public override Dictionary<string, CardModel> GenerateMoveToCardMap()
    {
        var card1 = CreateCardForIntent<Mince>();
        card1.SetDamage(MinceDamage);
        card1.SetRepeat(MinceHits);
        var card2 = CreateCardForIntent<BreakEgg>();
        card2.SetDamage(BreakEggDamage);
        card2.DynamicVars["Paralysis"].BaseValue = ParalysisAmt;
        var card3 = CreateCardForIntent<Sack>();
        var card4 = CreateCardForIntent<Season>();
        card4.SetWeak(AllyDebuffAmt);
        var card5 = CreateCardForIntent<Slap>();
        card5.SetBlock(BlockAmt);
        card5.DynamicVars["Bleed"].BaseValue = BleedAmt;
        var card6 = CreateCardForIntent<Trial>();
        card6.SetDamage(TrialDamage);
        card6.SetStrength(StrengthAmount);
        return new Dictionary<string, CardModel>
        {
            {MINCE, card1},
            {BREAK_EGG, card2},
            {SACK, card3},
            {SEASON, card4},
            {SLAP, card5},
            {TRIAL, card6}
        };
    }

    private async Task BreakEgg(IReadOnlyList<Creature> targets)
    {
        await BluntAnimation(targets);
        await DamageCmd.Attack(BreakEggDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await PowerCmd.Apply<Paralysis>(new ThrowingPlayerChoiceContext(), targets, ParalysisAmt, Creature,  null);
        await ResetIdle();
    }
    
    private async Task Mince(IReadOnlyList<Creature> targets)
    {
        for (int i = 0; i < MinceHits; i++)
        {
            if (i % 2 == 0)
            {
                await PierceAnimation(targets);
            }
            else
            {
                await SlashAnimation(targets);
            }
            await DamageCmd.Attack(MinceDamage)
                .FromMonsterCreature(this)
                .TargetingCreatures(targets, CombatState)
                .Execute(null);
            await ResetIdle();
        }
    }
    
    private async Task Slap(IReadOnlyList<Creature> targets)
    {
        await BlockAnimation(targets);
        await CreatureCmd.GainBlock(Creature, BlockAmt, ValueProp.Move, null);
        if (targets.Count > 0)
        {
            if (targets[0].IsPlayer)
            {
                await PowerCmd.Apply<Bleed>(new ThrowingPlayerChoiceContext(), targets, BleedAmt, Creature, null);
            }
            else
            {
                await ApplyPowerAndSkipNextDurationTick<BleedEnemy>(targets, BleedAmt);
            }
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Season(IReadOnlyList<Creature> targets)
    {
        await DebuffAnimation(targets);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, AllyDebuffAmt, Creature,  null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, AllyDebuffAmt, Creature,  null);
        await ResetIdle(1.0f);
    }
    
    private async Task Trial(IReadOnlyList<Creature> targets)
    {
        await Special1Animation(targets);
        var shouldGainStrength = targets.Count > 0 && targets[0].IsAlive;
        var attackCommand = await DamageCmd.Attack(TrialDamage)
            .FromMonsterCreature(this)
            .TargetingCreatures(targets, CombatState)
            .Execute(null);
        await VampireHeal(attackCommand);
        if (shouldGainStrength)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature,  null);
        }
        await ResetIdle(1.0f);
    }
    
    private async Task Sack(IReadOnlyList<Creature> targets)
    {
        await Special2Animation(targets);
        meat = await CreatureCmd.Add<FreshMeat>(CombatState, "minion");
        NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(meat);
        List<CardModel> cardsToSteal = new List<CardModel>();
        foreach (Creature target in targets)
        {
          if (!target.IsDead)
          {
            List<CardModel> list = CardPile.GetCards(target.Player ?? target.PetOwner, PileType.Draw, PileType.Discard).Where(c => c.DeckVersion != null).ToList();
            IEnumerable<CardModel> cardModels = list;
            foreach (Func<CardModel, bool> stealPriority in _stealPriorities)
            {
              IEnumerable<CardModel> source = list.Where(stealPriority);
              if (source.Any())
              {
                cardModels = source;
                break;
              }
            }
            if (cardModels.Any())
            {
              CardModel cardToSteal = RunRng.CombatCardGeneration.NextItem(cardModels);
              await CardPileCmd.RemoveFromCombat(cardToSteal);
              cardsToSteal.Add(cardToSteal);
              cardToSteal = null;
            }
          }
        }
        await Cmd.Wait(0.6f);
        foreach (CardModel card in cardsToSteal)
        {
          if (creatureNode != null && LocalContext.IsMine(card))
          {
            Marker2D? specialNode = creatureNode.GetSpecialNode<Marker2D>("%CenterPos");
            if (specialNode != null)
            {
              NCard? ncard = NCard.Create(card);
              if (ncard != null)
              {
                  specialNode.AddChildSafely(ncard);
                  ncard.Scale = new Vector2(0.6f, 0.6f);
                  ncard.Position = ncard.Position + ncard.Size * 0.5f;
                  ncard.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
              }
            }
          }
          FreshMeatPower freshMeatPower = (FreshMeatPower) ModelDb.Power<FreshMeatPower>().ToMutable();
          await freshMeatPower.Steal(card);
          if (card.Affliction != null)
          {
              CardCmd.ClearAffliction(card);
          }
          await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), freshMeatPower, meat, 1M, Creature, null);
        }
        await ResetIdle(1.0f);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature && OtherSideTargetMonster?.Monster is Hod hod)
        {
            if (hod.Creature.IsAlive)
            {
                await hod.OnBossDeath();
            }
        }
    }
    
    private async Task SlashAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Slash", Sfx.BluntVert, targets);
    }

    private async Task PierceAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Pierce", Sfx.BluntBlow, targets);
    }
    
    private async Task BluntAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Blunt", Sfx.BluntHori, targets);
    }
    
    private async Task Special1Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special1", Sfx.GretaEat, targets);
    }
    
    private async Task Special2Animation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Special2",null, targets);
    }
    
    private async Task BlockAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Block", null, targets);
    }
    
    private async Task DebuffAnimation(IReadOnlyList<Creature> targets)
    {
        await AnimationAction("Ranged", null, targets);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Blunt", "Pierce", "Slash", "Block", "Ranged", "Special1", "Special2"], controller);
    }
}