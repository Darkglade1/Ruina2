using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class LoveAndHate() : EGOCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), 
        new("firstThreshold", 4), new("firstThresholdBonus", 50), 
        new("secondThreshold", 6), new("secondThresholdBonus", 100)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int num = ResolveEnergyXValue();
        if (CombatState != null && num > 0)
        {
            decimal multipler = 1.0M;
            if (num >= DynamicVars["firstThreshold"].IntValue)
            {
                multipler *= 1.0M + (DynamicVars["firstThresholdBonus"].IntValue / 100M);
            }
            if (num >= DynamicVars["secondThreshold"].IntValue && IsUpgraded)
            {
                multipler *= 1.0M + (DynamicVars["secondThresholdBonus"].IntValue / 100M);
            }
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue * num * multipler).FromCard(this, play).TargetingAllOpponents(CombatState)
                .WithAttackerAnim("Cast", 0.5f).BeforeDamage(async delegate
                {
                    List<Creature> enemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
                    NHyperbeamVfx? nHyperbeamVfx = NHyperbeamVfx.Create(Owner.Creature, enemies.Last());
                    if (nHyperbeamVfx != null)
                    {
                        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamVfx);
                        await Cmd.Wait(0.5f);
                    }

                    foreach (Creature item in enemies)
                    {
                        NHyperbeamImpactVfx? nHyperbeamImpactVfx = NHyperbeamImpactVfx.Create(Owner.Creature, item);
                        if (nHyperbeamImpactVfx != null)
                        {
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamImpactVfx);
                        }
                    }
                }).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
    }
}