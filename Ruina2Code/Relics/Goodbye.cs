using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class Goodbye() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState!.TurnNumber > 1)
            return;
        Flash();
        await PowerCmd.Apply<GoodbyePower>(new ThrowingPlayerChoiceContext(), Owner.Creature,1, Owner.Creature, null);
    }
}