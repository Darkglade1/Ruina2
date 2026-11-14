using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using Ruina2.Ruina2Code.Cards.EGO;

namespace Ruina2.Ruina2Code.Merchant;

public sealed class BlitzMerchantEgoCardRewardEntry(Player player) : BlitzMerchantEntry(player)
{
    [CustomEnum] public static BlitzMerchantEntryType EgoCardReward;
    public override BlitzMerchantEntryType MerchantEntryType
    {
        get => BlitzMerchantEntryType.ColorlessCardReward; // lazy way of using the colorless card reward art assets
    }

    public override LocString Title => new LocString("merchant_room", "RUINA2-MERCHANT.blitz.ego_card_reward_entry.title");
    
    public override LocString Description =>  new LocString("merchant_room", "RUINA2-MERCHANT.blitz.ego_card_reward_entry.description");

    protected override async Task<bool> OnTrySelect()
    {
        if (Status != BlitzEntryStatus.Available)
            return false;
        CardReward cardReward = new CardReward(new CardCreationOptions([ModelDb.CardPool<EGOCardPool>()], CardCreationSource.Other, CardRarityOddsType.Blitz).WithRngOverride(_player.PlayerRng.Shops), 3, _player);
        await RewardsCmd.OfferCustom(_player, new List<Reward>(1)
        {
            cardReward
        });
        return true;
    }
}