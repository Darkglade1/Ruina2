using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Cards;
using Ruina2.Ruina2Code.Powers;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Messenger() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    private List<CardModel> prescripts = new List<CardModel>();

    private void PopulatePrescripts()
    {
        prescripts.Add(ModelDb.Card<AttackPrescript>());
        prescripts.Add(ModelDb.Card<SkillPrescript>());
        if (Owner.Monster != null)
        {
            prescripts.StableShuffle(Owner.Monster.Rng);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            Flash();
            if (prescripts.Count == 0)
            {
                PopulatePrescripts();
            }
            var prescript = prescripts[0];
            prescripts.RemoveAt(0);
            foreach (var player in CombatState.PlayerCreatures)
            {
                if (player.CombatState != null && player.Player != null)
                {
                    var card = player.CombatState.CreateCard(prescript, player.Player);
                    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, null);
                }
            }
        }
    }
}