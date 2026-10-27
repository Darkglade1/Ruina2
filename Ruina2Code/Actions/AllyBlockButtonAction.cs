using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Actions;

public sealed class AllyBlockButtonAction : GameAction
{
  public override ulong OwnerId => Player.NetId;

  public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

  public Player Player { get; }
  public ModelId ModelId { get; }
  public int Num { get; }

  public AllyBlockButtonAction(Player player, ModelId modelId, int num)
  {
    Player = player;
    ModelId = modelId;
    Num = num;
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
      if (Num == 1)
      {
        var blockToGive = Math.Min(NAllyBlockButton.BLOCK_TRANSFER, Player.Creature.Block);
        if (blockToGive > 0)
        {
          Player.Creature.Block -= blockToGive;
          await CreatureCmd.GainBlock(creature, blockToGive, ValueProp.Unpowered, null, true);
        }
      }
      else
      {
        var power = await PowerCmd.Apply<TransferBlockAlly>(new ThrowingPlayerChoiceContext(), Player.Creature, NAllyBlockButton.BLOCK_TRANSFER, creature, null, true);
        if (power != null)
        {
          var allyVar = power.DynamicVars["Ally"];
          if (allyVar is StringVar stringVar)
          {
            stringVar.StringValue = ModelId.Entry;
          } 
          var allyNameVar = power.DynamicVars["AllyName"];
          if (allyNameVar is StringVar stringNameVar)
          {
            stringNameVar.StringValue = creature.Name;
          } 
        }
      } 
    }
  }

  public override INetAction ToNetAction()
  {
    return new NetAllyBlockButtonAction
    {
      modelId = ModelId,
      num = Num
    };
  }

  public override string ToString()
  {
    return $"{nameof (AllyBlockButtonAction)}modelId: {ModelId}";
  }
}
