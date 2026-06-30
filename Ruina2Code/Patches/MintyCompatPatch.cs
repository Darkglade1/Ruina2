// using HarmonyLib;
// using MegaCrit.Sts2.Core.Entities.Creatures;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Modding;
// using MegaCrit.Sts2.Core.MonsterMoves.Intents;
// using MintySpire2.MintySpire2Code.combat;
// using Ruina2.Ruina2Code;
//
// [HarmonyPatch(typeof(SummedIncomingDamageRender), nameof(SummedIncomingDamageRender.CatchBarSet))]
// public static class MintyDamagePreviewPatch
// {
//     static bool Prepare()
//     {
//         foreach (var mod in ModManager.GetLoadedMods())
//         {
//             MainFile.Logger.Info("Loaded mod: " + mod);
//             MainFile.Logger.Info("Mod path: " + mod.path);
//         }
//         return true;
//     }
//     [HarmonyPrepare]
//     public static void Postfix()
//     {
//         MainFile.Logger.Info("Patch ran");
//     }
// }