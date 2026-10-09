using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Ruina2.Ruina2Code.Monsters.Act3.BigBird;

public sealed class Sage2 : Sage
{
    protected override void BattleEndTalk()
    {
        TalkCmd.Play(L10NMonsterLookup("RUINA2-SAGE.birdDeath2"), Creature, VfxColor.Black);
    }
}