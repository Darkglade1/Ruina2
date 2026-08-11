using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Ruina2.Ruina2Code.Monsters.Act3.BigBird;

public sealed class Sage2 : Sage
{
    public override async Task OnBigBirdDeath()
    {
        SetToSide(CombatSide.Enemy);
        await ResetIdle(0.5f);
        TalkCmd.Play(L10NMonsterLookup("RUINA2-SAGE.birdDeath2"), Creature, VfxColor.Black);
        await WaitAnimation(2.0f);
        await CreatureCmd.Kill(Creature);
    }
}