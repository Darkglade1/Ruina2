using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Ruina2.Ruina2Code.Cards;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Bliss() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<FragmentOfBliss>()];

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Owner)
        {
            foreach (var player in CombatState.Players)
            {
                var card = player.Creature.CombatState?.CreateCard<FragmentOfBliss>(player);
                if (card != null && !CombatManager.Instance.IsOverOrEnding)
                {
                    await CardPileCmd.AddGeneratedCardsToCombat([card], PileType.Hand, player);
                    await Cmd.Wait(0.25f);
                }
            }
        }
    }
}