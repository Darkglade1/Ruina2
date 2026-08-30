using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;

public class Harmony() : EGOCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public const int _baseStrength = 1;
    public int _currentStrength = 1;
    public int _increasedStrength;
    
    [SavedProperty]
    public int CurrentStrength
    {
        get => _currentStrength;
        set
        {
            AssertMutable();
            _currentStrength = value;
            DynamicVars.Strength.BaseValue = _currentStrength;
        }
    }

    [SavedProperty]
    public int IncreasedStrength
    {
        get => _increasedStrength;
        set
        {
            AssertMutable();
            _increasedStrength = value;
        }
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(11, ValueProp.Move), 
        new PowerVar<StrengthPower>(CurrentStrength), new ("Increase", 1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.Static(StaticHoverTip.Fatal)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (CombatState?.RunState.CurrentRoom is CombatRoom combatRoom)
        {
            ArgumentNullException.ThrowIfNull(play.Target, "cardPlay.Target");
            bool shouldTriggerFatal = play.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
            AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
            var targetKilled = attackCommand.Results.SelectMany(r => r)
                .Any((Func<DamageResult, bool>)(r => r.WasTargetKilled));
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.IntValue, Owner.Creature, this);
            if (shouldTriggerFatal && targetKilled)
            {
                int intValue = DynamicVars["Increase"].IntValue;
                BuffFromPlay(intValue);
                if (!(DeckVersion is Harmony deckVersion))
                    return;
                deckVersion.BuffFromPlay(intValue);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
    
    public void BuffFromPlay(int extraStrength)
    {
        IncreasedStrength += extraStrength;
        UpdateStrength();
    }

    public void UpdateStrength() => CurrentStrength = _baseStrength + IncreasedStrength;
}