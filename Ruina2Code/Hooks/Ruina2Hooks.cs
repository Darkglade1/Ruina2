using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace Ruina2.Ruina2Code.Hooks;

public class Ruina2Hooks
{
    private static async Task DispatchAsync<T>(IRunState? runState, ICombatState? combatState, Func<T, Task> action)
        where T : class
    {
        foreach (var model in runState?.IterateHookListeners(combatState).OfType<T>() ?? [])
        {
            await action(model);
        }
    }

    private static void Dispatch<T>(IRunState? runState, ICombatState? combatState, Action<T> action)
        where T : class
    {
        foreach (var model in runState?.IterateHookListeners(combatState).OfType<T>() ?? [])
        {
            action(model);
        }
    }

    public static Task AfterTransform(IRunState? rs, ICombatState? cs, IEnumerable<CardTransformation> transformations)
    {
        return DispatchAsync<IAfterTransform>(rs, cs, m => m.AfterTransform(transformations));
    }
}