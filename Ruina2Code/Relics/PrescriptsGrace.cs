using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class PrescriptsGrace() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            Flash();
            List<CardType> types = new List<CardType>()
            {
                CardType.Attack,
                CardType.Skill,
                CardType.Power
            };
            var chosenType = Owner.RunState.Rng.CombatCardGeneration.NextItem(types);
            if (chosenType == CardType.Attack)
            {
                await PowerCmd.Apply<PrescriptsGraceAttack>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            }
            if (chosenType == CardType.Skill)
            {
                await PowerCmd.Apply<PrescriptsGraceSkill>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            }
            if (chosenType == CardType.Power)
            {
                await PowerCmd.Apply<PrescriptsGracePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
            }
        }
    }
}