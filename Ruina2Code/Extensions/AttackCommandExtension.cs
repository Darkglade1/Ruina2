using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Ruina2.Ruina2Code.Extensions;

public static class AttackCommandExtensions
{
    public static AttackCommand FromMonsterCreature(this AttackCommand command, MonsterModel monster)
    {
        command.Attacker = command.Attacker == null
            ? monster.Creature
            : throw new InvalidOperationException("Attacker has already been set.");
        command._attackerAnimName = "Attack";
        command._sourceType = AttackCommand.SourceType.Monster;;
        return command;
    }
    
    public static AttackCommand TargetingCreatures(this AttackCommand command, IReadOnlyList<Creature> targets, ICombatState combatState)
    {
        if (targets[0].IsPlayer)
        {
            return command.TargetingAllOpponents(combatState);
        }
        return command.Targeting(targets[0]);
    }
}