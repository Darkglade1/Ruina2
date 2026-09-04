using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class Lumber() : EGOCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new CardsVar(2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
    
    public Decimal _extraDamage;
    public Decimal ExtraDamage
    {
        get => this._extraDamage;
        set
        {
            this.AssertMutable();
            this._extraDamage = value;
        }
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardAttack(this, play).Execute(choiceContext);
        var cards = await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, DynamicVars.Cards.IntValue), null, this);
        foreach (var card in cards)
        {
            if (card.Type == CardType.Attack)
            {
                Decimal damage1 = 0M;
                if (card.DynamicVars.ContainsKey("CalculatedDamage"))
                    damage1 = card.DynamicVars.CalculatedDamage.Calculate(null);
                else if (card.DynamicVars.ContainsKey("Damage"))
                    damage1 = card.DynamicVars.Damage.BaseValue;
                else if (card.DynamicVars.ContainsKey("OstyDamage"))
                    damage1 = card.DynamicVars.OstyDamage.BaseValue;
                Decimal num = Hook.ModifyDamage(card.Owner.RunState, card.Owner.Creature.CombatState, null, card.Owner.Creature, damage1, ValueProp.Move, card, null, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
                DamageVar damage2 = DynamicVars.Damage;
                damage2.BaseValue = damage2.BaseValue + num;
                ExtraDamage += num;
            }
            await CardCmd.Exhaust(choiceContext, card);
        }
    }
    
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DamageVar damage = this.DynamicVars.Damage;
        damage.BaseValue = damage.BaseValue + this.ExtraDamage;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}