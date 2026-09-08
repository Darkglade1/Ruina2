using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class Malice() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Unpowered), new DamageVar("IncreasedDamage",5, ValueProp.Unpowered), new("HPThreshold", 50)];
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
            return;
        Flash();
        if (player.Creature.CombatState != null)
        {
            if (player.Creature.CurrentHp <= player.Creature.MaxHp * (DynamicVars["HPThreshold"].BaseValue / 100M))
            {
                await CreatureCmd.Damage(choiceContext, player.Creature.CombatState.HittableEnemies, (DamageVar)DynamicVars["IncreasedDamage"], Owner.Creature);
            }
            else
            {
                await CreatureCmd.Damage(choiceContext, player.Creature.CombatState.HittableEnemies, DynamicVars.Damage, Owner.Creature);
            }
        }
    }
}