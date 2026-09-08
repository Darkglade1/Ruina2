using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class FragmentsFromSomewhere() : EGOCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(17, ValueProp.Move), new PowerVar<WeakPower>(1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            int targetBlock = play.Target.Block;
            if (targetBlock > 0)
            {
                await CreatureCmd.LoseBlock(choiceContext, play.Target, play.Target.Block, Owner.Creature);
                await CreatureCmd.GainBlock(Owner.Creature, targetBlock, ValueProp.Unpowered, play);
            }
            await CommonActions.CardAttack(this, play).Execute(choiceContext);
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, DynamicVars.Weak.IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}