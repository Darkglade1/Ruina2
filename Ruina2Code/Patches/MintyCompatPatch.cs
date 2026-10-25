using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using Ruina2.Ruina2Code.Intents;
using Ruina2.Ruina2Code.Monsters;
using Ruina2.Ruina2Code.Powers.UninvitedGuests;

namespace Ruina2.Ruina2Code.Patches;

// Patch for Minty's Damage summation to handle Ruina allies properly
public static class MintyCompatPatch
{
    private const string TargetAssemblyName = "MintySpire2";
    private const string TargetTypeName = "MintySpire2.MintySpire2Code.combat.SummedIncomingDamageRender";
    private const string TargetMethodName = "CalculateIncomingDamage";

    private static readonly object Lock = new();
    private static Harmony? _harmony;
    private static bool _patched;

    public static void Init(Harmony harmony)
    {
        lock (Lock)
        {
            if (_harmony != null) return;
            _harmony = harmony;
            
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (TryPatch(asm)) break;
            }
        }
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        lock (Lock)
        {
            TryPatch(args.LoadedAssembly);
        }
    }
    
    private static bool TryPatch(Assembly asm)
    {
        if (_patched) return true;
        if (_harmony == null) return false;
        if (asm.GetName().Name != TargetAssemblyName) return false;

        try
        {
            var type = asm.GetType(TargetTypeName, throwOnError: false);
            var target = type == null
                ? null
                : AccessTools.Method(type, TargetMethodName, [typeof(Creature)]);

            if (target == null)
            {
                MainFile.Logger.Info($"Minty found but {TargetTypeName}.{TargetMethodName} not found; skipping compat patch.");
            }
            else
            {
                _harmony.Patch(target, postfix: new HarmonyMethod(typeof(MintyCompatPatch), nameof(Postfix)));
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Info($"Failed to patch Minty's incoming damage: {e}");
        }
        
        _patched = true;
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        return true;
    }
    
    private static void Postfix(Creature __0, ref int __result)
    {
        __result = ModifyIncomingDamage(__0, __result);
    }

    private static int ModifyIncomingDamage(Creature creature, int original)
    {
        var damageToRemove = 0;
        var damageAdded = 0;
        if (creature.CombatState != null)
        {
            foreach (var enemy in creature.CombatState.Enemies.Where(enemy => enemy.IsAlive))
            {
                var enemyDamage = 0;
                if (enemy.Monster is AbstractAllyMonster ally)
                {
                    foreach (var intent in enemy.Monster.NextMove.Intents)
                    {
                        if (ally.MassAttackHitsPlayer && intent is RuinaMassAttackIntent)
                        {
                            continue;
                        }
                        if (intent.IntentType is IntentType.Attack)
                        {
                            enemyDamage += ((AttackIntent)intent).GetTotalDamage([creature], enemy);
                        }
                    }
                    damageToRemove += enemyDamage;
                } else if (enemy.Monster is AbstractMultiIntentMonster)
                {
                    foreach (var intent in enemy.Monster.NextMove.Intents)
                    {
                        Creature? targetCreature = RuinaAttackIntent.GetIntentTargetedCreature(intent, enemy);
                        if (intent.IntentType is IntentType.Attack && targetCreature != null && targetCreature.Player == null)
                        {
                            enemyDamage += ((AttackIntent)intent).GetTotalDamage([creature], enemy);
                        }
                    }
                    damageToRemove += enemyDamage;
                }
            }
            
            var vibration = creature.GetPower<Vibration>();
            if (vibration != null)
            {
                damageAdded += vibration.Amount;
            }
        }
        return original - damageToRemove + damageAdded;
    }
}