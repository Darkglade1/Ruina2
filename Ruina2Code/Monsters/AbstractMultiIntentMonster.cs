using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Ruina2.Ruina2Code.Monsters;

public abstract class AbstractMultiIntentMonster : AbstractRuinaMonster
{
    public List<MonsterMoveStateMachine>? ExtraIntentMoveStateMachines;
    public abstract List<MonsterMoveStateMachine> GenerateExtraIntentMoveStateMachine();
    public int NumExtraIntents { get; set; }
    public Creature? TargetCreature { get; set; }
}

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.SetUpForCombat))]
public static class GenerateExtraIntentStateMachinesPatch
{
    public static void Postfix(MonsterModel __instance)
    {
        if (__instance is AbstractMultiIntentMonster monster)
        {
            monster.ExtraIntentMoveStateMachines = monster.GenerateExtraIntentMoveStateMachine();
        }
    }
}