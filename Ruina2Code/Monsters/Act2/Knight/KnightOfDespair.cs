using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act2;

namespace Ruina2.Ruina2Code.Monsters.Act2.Knight;

public sealed class KnightOfDespair : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 180, 160);
    public override int MaxInitialHp => MinInitialHp;
    private int StrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int HPLossAmt => 40;

    protected override string VisualsPath => "Knight/knight.tscn".MonsterImagePath();

    private const string DESPAIR = "DESPAIR";
    private int MaxStabs = 4;
    private int stabs = 1;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Sfx.KnightChange.Play();
        await PowerCmd.Apply<Despair>(new ThrowingPlayerChoiceContext(), Creature, Creature.ScaleHpForMultiplayer(HPLossAmt, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex), Creature,  null);
    }

    private MoveState GetDespairState()
    {
        return new MoveState(DESPAIR, Despair, new SummonIntent(), new BuffIntent());
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var states = new List<MonsterState>();
        var state1 = GetDespairState();

        state1.FollowUpState = state1;

        states.Add(state1);
        
        return new MonsterMoveStateMachine(states, state1);
    }
    
    private async Task Despair(IReadOnlyList<Creature> targets)
    {
        bool foundSword = false;
        foreach (var creature in CombatState.HittableEnemies)
        {
            if (creature.Monster is Sword)
            {
                foundSword = true;
                Sfx.KnightGaho.Play();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, StrengthAmount, Creature,  null);
            }
        }

        if (!foundSword)
        {
            await CreatureCmd.Add<Sword>(CombatState, "sword");
        }
    }
    
    public override async Task BeforeDeath(Creature creature)
    {
        await base.BeforeDeath(creature);
        if (creature != Creature)
            return;

        var livingMinions = CombatState.GetTeammatesOf(Creature)
            .Where(t => t != Creature && t.IsAlive && t.Monster is Sword)
            .ToList();

        foreach (var minion in livingMinions)
        {
            await CreatureCmd.Kill(minion);
        }
    }


    public async Task OnSwordDeath()
    {
        stabs++;
        if (stabs >= MaxStabs)
        {
            stabs = MaxStabs;
        }
        await StabbedAnimation();
    }
    
    private async Task StabbedAnimation()
    {
        await AnimationAction("Idle" + stabs, Sfx.KnightAttack);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Idle2", "Idle3", "Idle4"], controller);
    }
}