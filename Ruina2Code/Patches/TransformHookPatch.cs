using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using Ruina2.Ruina2Code.Hooks;

namespace Ruina2.Ruina2Code.Patches;

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Transform), typeof(IEnumerable<CardTransformation>), typeof(Rng), typeof(CardPreviewStyle))]
public static class TransformHookPatch
{
    public static async Task<IEnumerable<CardPileAddResult>> Postfix(
        Task<IEnumerable<CardPileAddResult>> __result,
        IEnumerable<CardTransformation> transformations,
        Rng? rng,
        CardPreviewStyle style)
    {
        var results = await __result; // wait for original method to actually finish

        var cardTransformations = transformations.ToList();
        if (cardTransformations.Any())
        {
            var transformation = cardTransformations.First();
            await Ruina2Hooks.AfterTransform(
                transformation.Original.Owner.RunState,
                transformation.Original.Owner.Creature.CombatState,
                cardTransformations);
        }

        return results; // pass the original result back through unchanged
    }
}