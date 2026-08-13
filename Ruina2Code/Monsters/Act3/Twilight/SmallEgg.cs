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
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers.Act3;

namespace Ruina2.Ruina2Code.Monsters.Act3.Twilight;

public sealed class SmallEgg : AbstractRuinaMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 165, 150);
    public override int MaxInitialHp => MinInitialHp;

    protected override string VisualsPath => "Twilight/Eggs/small_egg.tscn".MonsterImagePath();
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        foreach (Creature target in CombatState.PlayerCreatures)
        {
            SmallBeak mutable = (SmallBeak) ModelDb.Power<SmallBeak>().ToMutable();
            mutable.Target = target;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), mutable, Creature, 2, Creature, null);
        }
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState initialState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine([initialState], initialState);
    }
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Creature)
        {
            await AnimationAction("Broken", null);
            await PowerCmd.Remove<SmallBeak>(Creature);
        }
    }
    
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        return creature != Creature;
    }
    
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        return GenerateAnimatorFromKeys(["Idle", "Broken"], controller);
    }
}