using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Relics;

[Pool(typeof(EventRelicPool))]
public class SeventhBullet : Ruina2Relic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Unpowered), new PowerVar<StrengthPower>(2), new CardsVar(7)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    private bool _isActivating;
    private int _attacksPlayed;
    
    public bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            UpdateDisplay();
        }
    }
    
    [SavedProperty]
    public int AttacksPlayed
    {
        get => _attacksPlayed;
        set
        {
            AssertMutable();
            _attacksPlayed = value;
            UpdateDisplay();
        }
    }
    
    public void UpdateDisplay()
    {
        if (IsActivating)
            Status = RelicStatus.Normal;
        else
            Status = AttacksPlayed == DynamicVars.Cards.IntValue - 1 ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }
    
    public override bool ShowCounter => true;
    
    public override int DisplayAmount => !IsActivating ? AttacksPlayed % DynamicVars.Cards.IntValue : DynamicVars.Cards.IntValue;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Strength.IntValue, Owner.Creature, null);
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && CombatManager.Instance.IsInProgress && cardPlay.Card.Type == CardType.Attack)
        {
            AttacksPlayed++;
            if (AttacksPlayed >= DynamicVars.Cards.IntValue)
            {
                TaskHelper.RunSafely(DoActivateVisuals());
                await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage, Owner.Creature);
                AttacksPlayed = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
    
    public async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();
        await Cmd.Wait(1f);
        IsActivating = false;
    }
}