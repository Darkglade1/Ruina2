using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Greta;

namespace Ruina2.Ruina2Code.Actions;

public sealed class PurpleTearStanceChangeAction : GameAction
{
  public override ulong OwnerId => Player.NetId;

  public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

  public Player Player { get; }
  public ModelId ModelId { get; }
  public int Stance { get; }

  public PurpleTearStanceChangeAction(Player player, ModelId modelId, int stance)
  {
    Player = player;
    ModelId = modelId;
    Stance = stance;
  }

  protected override async Task ExecuteAction()
  {
    if (Player.Creature.CombatState == null)
    {
      return;
    }
    var creature = Player.Creature.CombatState.Enemies.FirstOrDefault(c => c.ModelId == ModelId && c.IsAlive);
    if (creature?.Monster is Hod hod)
    {
      await hod.ChangeStance(Stance);
    }
  }

  public override INetAction ToNetAction()
  {
    return new NetPurpleTearStanceChangeAction
    {
      modelId = ModelId
    };
  }

  public override string ToString()
  {
    return $"{nameof (PurpleTearStanceChangeAction)}modelId: {ModelId}";
  }
}
