using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

namespace Ruina2.Ruina2Code.Powers.UninvitedGuests;

public class Spotlight() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;
    
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (Owner == creature && !wasRemovalPrevented)
        {
            await TransferToNextEnemy();
        }
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await TransferToNextEnemy();
        }
    }

    private async Task TransferToNextEnemy()
    {
        Creature? nextEnemy = null;
        if (Owner.Monster is Knife knife)
        {
            if (knife.mermaid != null && knife.mermaid.IsAlive)
            {
                nextEnemy = knife.mermaid;
            } else if (knife.oswald != null && knife.oswald.IsAlive)
            {
                nextEnemy = knife.oswald;
            }
        }
        if (Owner.Monster is Mermaid mermaid)
        {
            if (mermaid.oswald != null && mermaid.oswald.IsAlive)
            {
                nextEnemy = mermaid.oswald;
            } else if (mermaid.knife != null && mermaid.knife.IsAlive)
            {
                nextEnemy = mermaid.knife;
            } 
        }
        if (Owner.Monster is Oswald oswald)
        {
            if (oswald.knife != null && oswald.knife.IsAlive)
            {
                nextEnemy = oswald.knife;
            } else if (oswald.mermaid != null && oswald.mermaid.IsAlive)
            {
                nextEnemy = oswald.mermaid;
            }
        }
        if (nextEnemy != null)
        {
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<Spotlight>(new ThrowingPlayerChoiceContext(), nextEnemy, 1, nextEnemy,  null);
        }
    }
}