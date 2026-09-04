using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Powers.EGO;

public class FourthMatchFlamePower() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            Flash();
            NCombatRoom? instance = NCombatRoom.Instance;
            if (instance != null)
            {
                instance.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(Owner)!);
            }
            foreach (Creature hittableEnemy in CombatState.HittableEnemies)
            {
                NFireBurstVfx? child = NFireBurstVfx.Create(hittableEnemy, 0.75f);
                if (instance != null)
                {
                    instance.CombatVfxContainer.AddChildSafely(child);
                }
            }
            await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
            var burn = CombatState.CreateCard<Burn>(Owner.Player);
            burn.AddKeyword(CardKeyword.Ethereal);
            await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Hand, Owner.Player);
        }
    }
}