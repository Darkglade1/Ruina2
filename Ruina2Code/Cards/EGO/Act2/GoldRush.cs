using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Ruina2.Ruina2Code.Cards.EGO.Act2;

public class GoldRush() : EGOCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    
    protected override bool ShouldGlowGoldInternal => LastCardPlayedIsAttack;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature,
            DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
        if (LastCardPlayedIsAttack)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature,
                Owner.Creature.GetPowerAmount<StrengthPower>(), Owner.Creature, this);
        }
    }
    
    public bool LastCardPlayedIsAttack {
        get
        {
            List<CardPlayFinishedEntry> cardHistoryList = CombatManager.Instance.History.CardPlaysFinished.ToList();
            for (int i = cardHistoryList.Count - 1; i >= 0; i--)
            {
                CardModel card = cardHistoryList[i].CardPlay.Card;
                if (card.Owner == Owner)
                {
                    if (card.Type == CardType.Attack)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(2);
    }
}