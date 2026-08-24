using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Actions;

public sealed class AllyBlockButtonAction : GameAction
{
  public override ulong OwnerId => Player.NetId;

  public override GameActionType ActionType => GameActionType.Combat;

  public Player Player { get; }
  public ModelId ModelId { get; }

  public AllyBlockButtonAction(Player player, ModelId modelId)
  {
    Player = player;
    ModelId = modelId;
  }

  protected override async Task ExecuteAction()
  {
    if (Player.Creature.CombatState == null)
    {
      return;
    }
    var creature = Player.Creature.CombatState.Enemies.FirstOrDefault(c => c.ModelId == ModelId && c.IsAlive);
    if (creature?.Monster is AbstractAllyMonster)
    {
      var blockToGive = Math.Min(NAllyBlockButton.BLOCK_TRANSFER, Player.Creature.Block);
      if (blockToGive > 0)
      {
        Player.Creature.Block -= blockToGive;
        await CreatureCmd.GainBlock(creature, blockToGive, ValueProp.Unpowered, null, true);
      }
    }
  }

  public override INetAction ToNetAction()
  {
    return new NetAllyBlockButtonAction
    {
      modelId = ModelId
    };
  }

  public override string ToString()
  {
    return $"{nameof (AllyBlockButtonAction)}modelId: {ModelId}";
  }
}
