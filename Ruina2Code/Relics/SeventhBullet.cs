using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class SeventhBullet() : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Unpowered), new PowerVar<StrengthPower>(2), new CardsVar(7)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    private int _attacksPlayed;
    
    public override bool ShowCounter => true;
    
    public override int DisplayAmount => _attacksPlayed;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Strength.IntValue, Owner.Creature, null);
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && CombatManager.Instance.IsInProgress && cardPlay.Card.Type == CardType.Attack)
        {
            _attacksPlayed++;
            if (_attacksPlayed >= DynamicVars.Cards.IntValue)
            {
                Flash();
                _attacksPlayed = 0;
                await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage, Owner.Creature);
            }
            InvokeDisplayAmountChanged();
        }
    }
}