using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards;

[Pool(typeof(StatusCardPool))]
public class Apostle() : Ruina2Card(1, CardType.Status,
    CardRarity.Status, TargetType.Self)
{
    public int _fakeUpgradeLevel;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2, ValueProp.Unpowered | ValueProp.Move)];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    { 
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage, this, null);
    }
    
    public override string Title
    {
        get
        {
            string title = base.Title;
            if (this.FakeUpgradeLevel <= 0)
                return title;
            return $"{title}+{this.FakeUpgradeLevel}";
        }
    }

    public int FakeUpgradeLevel
    {
        get => this._fakeUpgradeLevel;
        set
        {
            this.AssertMutable();
            this._fakeUpgradeLevel = value;
        }
    }
    
    public void FakeUpgrade()
    {
        ++this.FakeUpgradeLevel;
        this.DynamicVars.Damage.UpgradeValueBy(2);
    }

    public override int MaxUpgradeLevel => 0;
}