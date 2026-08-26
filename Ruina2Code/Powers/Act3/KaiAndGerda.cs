using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class KaiAndGerda() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Frozen>();

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Owner)
        {
            Sfx.SnowPrisonBreak.Play(0, 0.4f);
            foreach (var player in CombatState.Players)
            {
                if (player.PlayerCombatState != null)
                {
                    foreach (CardModel card in player.PlayerCombatState.AllCards.Where(c => c.Affliction is Frozen))
                    {
                        CardCmd.ClearAffliction(card);
                    }
                }
            }
        }
    }
}