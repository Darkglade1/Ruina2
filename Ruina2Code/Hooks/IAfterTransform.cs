using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Ruina2.Ruina2Code.Hooks;

public interface IAfterTransform
{
    Task AfterTransform(IEnumerable<CardTransformation> transformations);
}