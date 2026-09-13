using BaseLib.Patches.Features;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Ruina2.Ruina2Code.Afflictions;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Oswald;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ModelDb), "Init")]
internal static class ModelDbTargetTypeInitPatch
{
    [HarmonyPostfix]
    private static void RegisterTargetTypes()
    {
        CustomTargetType.RegisterSingleTargetType(Brainwash.AnyRuinaAlly,
            (target) => target.Monster != null && target.Monster is AbstractAllyMonster && target is { IsAlive: true, IsPet: false });
    }
}

[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay.SingleCreatureTargeting))]
public static class NMouseCardPlayPatch
{
    public static void Prefix(NMouseCardPlay __instance, TargetMode targetMode, TargetType targetType)
    {
        if (__instance.Player.Creature.CombatState != null)
        {
            if (__instance.Holder.CardModel != null && __instance.Holder.CardModel.Affliction is Brainwash)
            {
                foreach (var enemy in __instance.Player.Creature.CombatState.Enemies)
                {
                    if (enemy.Monster is Tiph tiph)
                    {
                        if (tiph.IsTargetableByPlayersMutable)
                        {
                            tiph.IsTargetableByPlayers = true;
                        }
                    }
                }
            }
            if (__instance.Holder.CardModel != null && !(__instance.Holder.CardModel.Affliction is Brainwash))
            {
                foreach (var enemy in __instance.Player.Creature.CombatState.Enemies)
                {
                    if (enemy.Monster is Tiph tiph)
                    {
                        if (tiph.IsTargetableByPlayersMutable)
                        {
                            tiph.IsTargetableByPlayers = false;
                        }
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay.SingleCreatureTargeting))]
public static class NControllerCardPlayPatch
{
    public static void Prefix(NControllerCardPlay __instance, TargetType targetType)
    {
        if (__instance.Player.Creature.CombatState != null)
        {
            if (__instance.Holder.CardModel != null && __instance.Holder.CardModel.Affliction is Brainwash)
            {
                foreach (var enemy in __instance.Player.Creature.CombatState.Enemies)
                {
                    if (enemy.Monster is Tiph tiph)
                    {
                        if (tiph.IsTargetableByPlayersMutable)
                        {
                            tiph.IsTargetableByPlayers = true;
                        }
                    }
                }
            }
            if (__instance.Holder.CardModel != null && !(__instance.Holder.CardModel.Affliction is Brainwash))
            {
                foreach (var enemy in __instance.Player.Creature.CombatState.Enemies)
                {
                    if (enemy.Monster is Tiph tiph)
                    {
                        if (tiph.IsTargetableByPlayersMutable)
                        {
                            tiph.IsTargetableByPlayers = false;
                        }
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.CancelPlayCard))]
public static class NCardPlayCancelPlayCardPatch
{
    public static void Postfix(NCardPlay __instance)
    {
        if (__instance.Player.Creature.CombatState != null)
        {
            if (__instance.Holder.CardModel != null && __instance.Holder.CardModel.Affliction is Brainwash)
            {
                foreach (var enemy in __instance.Player.Creature.CombatState.Enemies)
                {
                    if (enemy.Monster is Tiph tiph)
                    {
                        if (tiph.IsTargetableByPlayersMutable)
                        {
                            tiph.IsTargetableByPlayers = false;
                        }
                    }
                }
            }
        }
    }
}