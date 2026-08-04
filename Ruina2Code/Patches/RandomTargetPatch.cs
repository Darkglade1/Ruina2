// using System.Collections;
// using System.Reflection;
// using HarmonyLib;
// using MegaCrit.Sts2.Core.Entities.Creatures;
// using MegaCrit.Sts2.Core.Random;
// using Ruina2.Ruina2Code.Powers;
//
// namespace Ruina2.Ruina2Code.Patches;
//
// [HarmonyPatch]
// public static class RandomTargetSelectCenterOfAttention
// {
//     static MethodBase TargetMethod()
//     {
//         var openMethod = AccessTools.Method(typeof(Rng), "NextItem");
//         return openMethod.MakeGenericMethod(typeof(Creature));
//     }
//     public static bool Prefix(Rng __instance, object items, ref object __result)
//     {
//         MainFile.Logger.Info($"[RandomTargetSelectCenterOfAttention] {items}");
//         // Fast path: if it's a real Array, use Array's own non-generic
//         // Length/GetValue — these are core runtime methods, never
//         // subject to per-T trimming, since Array handles element
//         // access without any closed-generic codegen.
//         if (items is Array arr)
//         {
//             for (int i = 0; i < arr.Length; i++)
//             {
//                 var item = arr.GetValue(i);
//                 if (item is Creature creature)
//                 {
//                     MainFile.Logger.Info($"[RandomTargetSelectCenterOfAttention] {creature.Name}");
//                     if (creature.HasPower<CenterOfAttention>())
//                     {
//                         MainFile.Logger.Info($"[RandomTargetSelectCenterOfAttention] targeted {creature.Name}");
//                         __result = item;
//                         return false;
//                     }
//                 }
//                 else
//                 {
//                     int maxExclusive = arr.Length;
//                     int index = __instance.NextInt(0, maxExclusive);
//                     __result = arr.GetValue(index)!;
//                     return false;
//                 }
//             }
//         }
//
//         // Fallback: fully reflection-driven, non-generic-typed enumeration.
//         // Using the OBJECT's actual runtime type + non-generic IEnumerable
//         // avoids emitting a direct call to any specific closed generic
//         // method that the trimmer might have stripped.
//         if (items is IEnumerable nonGeneric)
//         {
//             int count = 0;
//             foreach (object o in nonGeneric)
//             {
//                 if (o is Creature creature)
//                 {
//                     MainFile.Logger.Info($"[RandomTargetSelectCenterOfAttention] {creature.Name}");
//                     if (creature.HasPower<CenterOfAttention>())
//                     {
//                         MainFile.Logger.Info($"[RandomTargetSelectCenterOfAttention] targeted {creature.Name}");
//                         __result = o;
//                         return false;
//                     }
//                 }
//                 count++;
//             }
//             int index = __instance.NextInt(0, count);
//             int count2 = 0;
//             foreach (object o in nonGeneric)
//             {
//                 if (index == count2)
//                 {
//                     __result = o;
//                     return false;
//                 }
//                 count2++;
//             }
//         }
//         
//         return false;
//     }
// }