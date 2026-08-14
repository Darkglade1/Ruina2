using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Afflictions;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.SnowQueen;

public sealed class PrisonOfIce : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 75, 70);
    public override int MaxInitialHp => MinInitialHp;

    protected override string VisualsPath => "SnowQueen/Prison/prison.tscn".MonsterImagePath();
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<KaiAndGerda>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState initialState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine([initialState], initialState);
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle"], controller);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature)
        {
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is SnowQueen queen)
                {
                    queen.CanBlizzard = true;
                }   
            }
        }
    }
}