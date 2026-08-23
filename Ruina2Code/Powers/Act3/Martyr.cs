using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Monsters.Act3.BlueStar;

namespace Ruina2.Ruina2Code.Powers.Act3;

public class Martyr() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature == Owner && creature.Monster is Worshipper worshipper && worshipper.TriggerMartyr)
        {
            NCombatRoom? instance = NCombatRoom.Instance;
            if (instance != null && instance.GetCreatureNode(Owner) != null)
            {
                instance.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(Owner)); 
            }
            await Cmd.CustomScaledWait(0.2f, 0.3f);
        }
    }

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature == Owner && creature.Monster is Worshipper worshipper && worshipper.TriggerMartyr)
        {
            Sfx.WorshipperExplode.Play();
            Creature? shrine = null;
            // make blue star the dealer since dead dealers can't deal damage
            foreach (var enemy in CombatState.HittableEnemies)
            {
                if (enemy.Monster is BlueStar)
                {
                    shrine = enemy;
                }
            }
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), CombatState.PlayerCreatures, Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, shrine, null, null);
        }
    }
}