using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Ruina2.Ruina2Code.Audio;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class BlindRage() : EGOCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(26, ValueProp.Move), 
        new("SelfDamage", 6), new("HpThreshold", 50)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    
    protected override bool ShouldGlowGoldInternal => BelowHPThreshold;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        Sfx.WrathStrong3.Play(0.0f, 0.8f);
        await CommonActions.CardAttack(this, play).Execute(choiceContext);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars["SelfDamage"].IntValue, ValueProp.Unpowered, Owner.Creature, this, play);
    }
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        if (card == this)
        {
            if (BelowHPThreshold)
            {
                modifiedCost = 0M;
                return true;
            }   
        }
        modifiedCost = originalCost;
        return false;
    }
    
    public bool BelowHPThreshold => Owner.Creature.CurrentHp <= Owner.Creature.MaxHp * (DynamicVars["HpThreshold"].BaseValue / 100M);

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["SelfDamage"].UpgradeValueBy(-2);
    }
}