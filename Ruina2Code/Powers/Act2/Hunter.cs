using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters.Act2;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Hunter() : Ruina2Power
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ("HPThreshold", 50), new ("Intangible", 1)];
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (Owner.CurrentHp < Owner.MaxHp * (DynamicVars["HPThreshold"].BaseValue / 100) && Owner.Monster is BadWolf wolf && !wolf.powerTriggered)
            {
                await wolf.SetPhase(2);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner,  null);
                await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Owner, DynamicVars["Intangible"].BaseValue, Owner,  null);
            }
        }
    }
    
    public override async Task AfterSideTurnEndLate(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            {
                if (Owner.Monster is BadWolf badWolf && !Owner.HasPower<IntangiblePower>() && badWolf.phase == 2)
                {
                    await badWolf.SetPhase(1);
                }
            }
        }
    }
    
}