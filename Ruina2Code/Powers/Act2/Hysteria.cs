using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Ruina2.Ruina2Code.Monsters.Act2;

namespace Ruina2.Ruina2Code.Powers.Act2;

public class Hysteria() : Ruina2Power, IHasSecondAmount
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    
    public override int DisplayAmount => DynamicVars["SkillCounter"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3), 
        new PowerVar<WeakPower>(2), new PowerVar<FrailPower>(2),
        new("AttackCounter",0), new("SkillCounter",0)];
    
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Target)
        {
            DynamicVars["AttackCounter"].BaseValue = 0;
            DynamicVars["SkillCounter"].BaseValue = 0;
            InvokeDisplayAmountChanged();
            this.InvokeSecondAmountChanged();
        }
        return Task.CompletedTask;
    }
    
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (applier == Owner && power is Hysteria)
        {
            DynamicVars["AttackCounter"].BaseValue = 0;
            DynamicVars["SkillCounter"].BaseValue = 0;
            InvokeDisplayAmountChanged();
            this.InvokeSecondAmountChanged();
        }
        return Task.CompletedTask;
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Target)
        {
            if (cardPlay.Card.Type == CardType.Attack)
            {
                DynamicVars["AttackCounter"].BaseValue += 1;
                if (DynamicVars["AttackCounter"].BaseValue >= DynamicVars.Cards.IntValue)
                {
                    Flash();
                    await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), Target, DynamicVars["WeakPower"].IntValue, Owner,  null);
                    DynamicVars["AttackCounter"].BaseValue = 0;
                    if (Owner.Monster is QueenOfHate queen)
                    {
                        if (!queen.hysteriaTriggered)
                        {
                            queen.hysteriaTriggered = true;
                            queen.hysteriaJustTriggered = true;
                        }
                    }
                }
                this.InvokeSecondAmountChanged();
            }
            if (cardPlay.Card.Type == CardType.Skill)
            {
                DynamicVars["SkillCounter"].BaseValue += 1;
                if (DynamicVars["SkillCounter"].BaseValue >= DynamicVars.Cards.IntValue)
                {
                    Flash();
                    await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), Target, DynamicVars["FrailPower"].IntValue, Owner,  null);
                    DynamicVars["SkillCounter"].BaseValue = 0;
                    if (Owner.Monster is QueenOfHate queen)
                    {
                        if (!queen.hysteriaTriggered)
                        {
                            queen.hysteriaTriggered = true;
                            queen.hysteriaJustTriggered = true;
                        }
                    }
                }
                InvokeDisplayAmountChanged();
            }   
        }
    }
    
    public string GetSecondAmount()
    {
        return DynamicVars["AttackCounter"].IntValue.ToString();
    }
}