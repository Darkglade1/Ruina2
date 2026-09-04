using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Powers.EGO;

namespace Ruina2.Ruina2Code.Cards.EGO.Act3;

public class DeadSilence() : EGOCard(4,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move), new EnergyVar(2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var attackCommand = await CommonActions.CardAttack(this, play).Execute(choiceContext);
        var targetKilled = attackCommand.Results.SelectMany(r => r).Any((Func<DamageResult, bool>)(r => r.WasTargetKilled));
        if (targetKilled)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this); 
        }
        await PowerCmd.Apply<DeadSilencePower>(new ThrowingPlayerChoiceContext(), Owner.Creature,1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}