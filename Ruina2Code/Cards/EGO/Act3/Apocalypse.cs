using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act3;

public class Apocalypse() : EGOCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move), 
        new("PlayerHPThreshold", 50), new("EnemyHPThreshold", 50)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoomPower>()];
    
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            return (CombatState != null && CombatState.HittableEnemies.Any(e =>
            {
                return e.CurrentHp <= e.MaxHp * (DynamicVars["EnemyHPThreshold"].BaseValue / 100M);
            })) || PlayerBelowHPThreshold;
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target != null)
        {
            var enemyBelowHPThreshold = play.Target.CurrentHp <= play.Target.MaxHp * (DynamicVars["EnemyHPThreshold"].BaseValue / 100M);
            var attackCommand = await CommonActions.CardAttack(this, play).Execute(choiceContext);
            if (enemyBelowHPThreshold)
            {
                var damageDone = attackCommand.Results
                    .SelectMany(r => r)
                    .Sum((Func<DamageResult, int>)(r => r.TotalDamage));
                await PowerCmd.Apply<DoomPower>(new ThrowingPlayerChoiceContext(), play.Target, damageDone , Owner.Creature, this);
            }
        }
        
    }
    
    public override decimal ModifyDamageMultiplicative
    (
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource == this && dealer == Owner.Creature && props.IsPoweredAttack())
        {
            if (PlayerBelowHPThreshold)
            {
                return 2M;
            }
        }
        return 1M;
    }
    
    public bool PlayerBelowHPThreshold => Owner.Creature.CurrentHp <= Owner.Creature.MaxHp * (DynamicVars["PlayerHPThreshold"].BaseValue / 100M);

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}