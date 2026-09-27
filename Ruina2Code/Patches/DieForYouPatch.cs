using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Encounters.UninvitedGuests;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Patches;

// Make Tiph take priority over Osty
[HarmonyPatch(typeof(DieForYouPower), nameof(DieForYouPower.ModifyUnblockedDamageTarget))]
public static class DieForYouPatch
{
    public static void Postfix(DieForYouPower __instance, Creature target,
        ValueProp props,
        ref Creature __result)
    {
        var localPlayer = LocalContext.GetMe(RunManager.Instance.State);
        var encounter = localPlayer?.Creature.CombatState?.Encounter;
        if (encounter is OswaldEncounter)
        {
            foreach (var enemy in localPlayer.Creature.CombatState.Enemies)
            {
                if (enemy.Monster is Tiph && enemy.IsAlive && enemy.HasPower<HanaForAll>())
                {
                    if (target.IsPlayer && props.IsPoweredAttack() && localPlayer.Creature.CombatState?.CurrentSide == CombatSide.Player)
                    {
                        __result = enemy;
                    }
                }
            }
        }
    }
}