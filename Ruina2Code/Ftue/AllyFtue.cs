using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters;

namespace Ruina2.Ruina2Code.Ftue;

public class AllyFtue() : CustomSingletonModel(HookType.Combat)
{
    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (Config.ViewAllyFtue && LocalContext.IsMe(card.Owner))
        {
            if (card.Owner.Creature.CombatState != null)
            {
                foreach (var enemy in card.Owner.Creature.CombatState.Enemies)
                {
                    if (enemy.Monster is AbstractAllyMonster)
                    {
                        return TaskHelper.RunSafely(ShowCombatFtue());
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    private static async Task ShowCombatFtue()
    {
        SceneTree? tree = NModalContainer.Instance != null ? NModalContainer.Instance.GetTree() : null;
        if (tree == null)
        {
            return;
        }
        if (((NModalContainer.Instance != null) ? NModalContainer.Instance.OpenModal : null) == null)
        {
            NAllyFtue nAllyFtue = NAllyFtue.Create("Ally", ["RUINA2-ALLY-FTUE.title"], ["RUINA2-ALLY-FTUE.body"], ["ally_tutorial.png".UIImagePath()]);
            if (NModalContainer.Instance != null)
            {
                NModalContainer.Instance.Add(nAllyFtue);
                Config.ViewAllyFtue = false;
            }
        }
    }
}