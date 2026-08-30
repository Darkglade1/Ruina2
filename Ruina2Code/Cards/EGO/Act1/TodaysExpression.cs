using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Ruina2.Ruina2Code.Cards.EGO.Act1;
public class TodaysExpression() : EGOCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15, ValueProp.Move), new PowerVar<StrengthPower>(3)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var shyLook = (CardModel)ModelDb.Card<ShyLook>().MutableClone();
        var fierceLook = (CardModel)ModelDb.Card<FierceLook>().MutableClone();
        shyLook.GetDynamicVar("Block").BaseValue = Hook.ModifyBlock(CombatState!, Owner.Creature, shyLook.GetDynamicVar("Block").BaseValue, ValueProp.Move, this,
            play, out IEnumerable<AbstractModel> models);
        var cardsToChoose = new CardModel[]
        {
            shyLook,
            fierceLook
        }.Select(e => e).ToList();

        foreach (var c in cardsToChoose)
        {
            c.Owner = Owner;
            if (IsUpgraded)
                CardCmd.Upgrade(c);
        }

        var card = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            cardsToChoose,
            Owner
        );
        if (card == null)
        {
            return;
        }
        await ((IChoosable) card).OnChosen(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars.Strength.UpgradeValueBy(1);
    }
    
    public interface IChoosable
    {
        Task OnChosen(PlayerChoiceContext choiceContext, CardPlay play);
    }
}