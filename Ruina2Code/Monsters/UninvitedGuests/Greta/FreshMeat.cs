using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Monsters.UninvitedGuests.Greta;

public sealed class FreshMeat : AbstractMultiIntentMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 110, 100);
    public override int MaxInitialHp => MinInitialHp;
    public override string TargetTexturePath => "MeatIcon.png".UIImagePath();

    protected override string VisualsPath => "FreshMeat/fresh_meat.tscn".MonsterImagePath();
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        if (CombatState.Players.Count > 1)
        {
            await PowerCmd.Apply<MultiplayerAlly>(new ThrowingPlayerChoiceContext(), Creature, AbstractAllyMonster.GetAllyMultiplayerDamageModifier(CombatState), Creature,  null);
        }
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    
    private MonsterMoveStateMachine GenerateIntent1StateMachine()
    {
        MoveState emptyState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        emptyState.FollowUpState = emptyState;
        MoveState initialState = emptyState;
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { initialState },
            initialState
        );
    }


    public override List<MonsterMoveStateMachine> GenerateMultiIntentMoveStateMachine()
    {
        return [GenerateIntent1StateMachine()];
    }

    public override Creature DetermineTargetForIntent(int intentNum)
    {
        return CombatState.PlayerCreatures[0];
    }
}