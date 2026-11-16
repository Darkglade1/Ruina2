using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(ArrowCmd), nameof(ArrowCmd.ShootArrowAtTarget),
    new[] { typeof(Creature), typeof(Player), typeof(AttackCommand), typeof(PlayerChoiceContext) })]
public static class ArrowCmdPatch
{
    public static void Prefix(Creature target,
        Player player,
        AttackCommand attackCommand,
        PlayerChoiceContext context)
    {
        if (target.Monster is AbstractAllyMonster allyMonster)
        {
            allyMonster.CanApplyPowersToAllies = true;
        }
    }
}

[HarmonyPatch(typeof(ArrowCmd), nameof(ArrowCmd.ShootArrowAtTarget),
    new[] { typeof(Creature), typeof(Player), typeof(PlayerChoiceContext), typeof(CardModel), typeof(Func<Task>) })]
public static class ArrowCmdPatch2
{
    public static void Prefix(Creature target,
        Player player,
        PlayerChoiceContext context,
        CardModel? card,
        Func<Task>? actionToPerform)
    {
        if (target.Monster is AbstractAllyMonster allyMonster)
        {
            allyMonster.CanApplyPowersToAllies = true;
        }
    }
}