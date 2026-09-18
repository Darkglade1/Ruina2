using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace Ruina2.Ruina2Code.Actions;

public struct NetPurpleTearStanceChangeAction : INetAction
{
  public ModelId modelId;
  public int stance;

  public GameAction ToGameAction(Player player)
  {
    return new PurpleTearStanceChangeAction(player, modelId, stance);
  }

  public void Serialize(PacketWriter writer)
  {
    writer.WriteModelEntry(modelId);
    writer.WriteInt(stance);
  }

  public void Deserialize(PacketReader reader)
  {
    modelId = reader.ReadModelIdAssumingType<MonsterModel>();
    stance = reader.ReadInt();
  }

  public override string ToString()
  {
    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
    interpolatedStringHandler.AppendLiteral("NetPurpleTearStanceChangeAction:");
    interpolatedStringHandler.AppendLiteral("\nmodelId is " + modelId);
    return interpolatedStringHandler.ToStringAndClear();
  }
}
