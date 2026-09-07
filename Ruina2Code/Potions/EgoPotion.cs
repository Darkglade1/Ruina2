using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Potions;

[Pool(typeof(SharedPotionPool))]
public class EgoPotion : Ruina2Potion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    
    public override IEnumerable<IHoverTip> ExtraHoverTips => [];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        if (target.Player == null) return;
        Player player = target.Player;

        if (player.Creature.CombatState != null)
        {
            NCombatRoom.Instance?.PlaySplashVfx(target, new Color("3c2d26"));
            var egoCards1 = EGOCardPool.GetAct1EgoCards();
            egoCards1.StableShuffle(player.RunState.Rng.CombatCardGeneration);
            var egoCards2 = EGOCardPool.GetAct2EgoCards();
            egoCards2.StableShuffle(player.RunState.Rng.CombatCardGeneration);
            var egoCards3 = EGOCardPool.GetAct3EgoCards();
            egoCards3.StableShuffle(player.RunState.Rng.CombatCardGeneration);
        
            List<CardModel> options = new List<CardModel>();
            options.Add(player.Creature.CombatState.CreateCard(egoCards1[0], player));
            options.Add(player.Creature.CombatState.CreateCard(egoCards2[0], player));
            options.Add(player.Creature.CombatState.CreateCard(egoCards3[0], player));
            CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, player, true);
            if (chosenCard != null)
            {
                chosenCard.SetToFreeThisTurn();
                await CardPileCmd.AddGeneratedCardToCombat(chosenCard, PileType.Hand, Owner);
            }
        }
    }
}