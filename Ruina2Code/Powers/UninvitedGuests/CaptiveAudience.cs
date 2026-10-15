using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class CaptiveAudience() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Spotlight>()];
    
    public override async Task BeforeCombatStart()
    {
        if (Target != null && Target.Player != null && Target.Player.PlayerCombatState != null)
        {
            foreach (CardModel card in Target.Player.PlayerCombatState.AllCards.Where(c => c.TargetType == TargetType.AnyEnemy))
            {
                await CardCmd.Afflict<Captivated>(card, Amount);
            }
        }
    }
    
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (Target != null && Target.Player == card.Owner && card.Affliction == null && card.TargetType == TargetType.AnyEnemy)
        {
            await CardCmd.Afflict<Captivated>(card, Amount);
        }
    }
    
    public override Task AfterRemoved(Creature oldOwner)
    {
        if (Target != null && Target.Player != null && Target.Player.PlayerCombatState != null)
        {
            foreach (CardModel card in Target.Player.PlayerCombatState.AllCards.Where(c => c.Affliction is Captivated))
            {
                CardCmd.ClearAffliction(card);
            }
        }
        return Task.CompletedTask;
    }
}