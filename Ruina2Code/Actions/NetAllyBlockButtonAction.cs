using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace Ruina2.Ruina2Code.Actions;

public struct NetAllyBlockButtonAction : INetAction
{
  public ModelId modelId;

  public GameAction ToGameAction(Player player)
  {
    return new AllyBlockButtonAction(player, modelId);
  }

  public void Serialize(PacketWriter writer)
  {
    writer.WriteModelEntry(modelId);
  }

  public void Deserialize(PacketReader reader)
  {
    modelId = reader.ReadModelIdAssumingType<MonsterModel>();
  }

  public override string ToString()
  {
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
    interpolatedStringHandler.AppendLiteral("NetAllyBlockButtonAction:");
    interpolatedStringHandler.AppendLiteral("\nmodelId is " + modelId);
    return interpolatedStringHandler.ToStringAndClear();
  }
}
