using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Afflictions;

namespace Ruina2.Ruina2Code.Powers.Act1;

public class WintersInception() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    private bool hasTriggered;
    
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target == Owner && props.IsPoweredAttack() && dealer != null && dealer == Target && dealer.Player != null && cardSource != null)
        {
            if (!hasTriggered || cardSource.Affliction is LaurelWreath)
            {
                return 1M;
            }
            return 1M - (decimal)Amount / 100;
        }
        return 1M;
    }
    
    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker == Target && command.CardPlay != null && command.CardPlay.Card.Type == CardType.Attack)
        {
            if (!(command.CardPlay.Card.Affliction is LaurelWreath) && !hasTriggered)
            {
                Flash();
                hasTriggered = true;
                await CardCmd.Afflict<LaurelWreath>(command.CardPlay.Card, 1);
            }
        }
    }
    
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Target)
        {
            hasTriggered = false;
        }
        return Task.CompletedTask;
    }
}