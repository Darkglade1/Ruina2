using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Relics;

namespace Ruina2.Ruina2Code.Cards.Quests;

[Pool(typeof(QuestCardPool))]
public class PrescriptAshes() : Ruina2Card(-1, CardType.Quest,
    CardRarity.Quest, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public override int MaxUpgradeLevel => 0;
    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (Owner.Creature.IsDead || !(room is MerchantRoom))
            return;
        List<CardModel> list = PileType.Deck.GetPile(Owner).Cards.Where(c => c is PrescriptAshes).ToList();
        if (list.Count > 0)
        {
            Sfx.IndexUnlock.Play(1, 0.5f);
            await CardPileCmd.RemoveFromDeck(list);
            await RelicCmd.Obtain(ModelDb.Relic<PrescriptsGrace>().ToMutable(), Owner);
            if (Owner.RunState.CurrentRoom is MerchantRoom)
            {
                NMerchantRoom? merchantRoom = NRun.Instance?.MerchantRoom;
                if (merchantRoom != null)
                {
                    SfxCmd.Play("event:/sfx/npcs/merchant/merchant_thank_yous");
                    LocString line1 = new LocString("merchant_room", "RUINA2-MERCHANT.talk.ashes.line1");
                    merchantRoom.MerchantButton.PlayDialogue(line1);
                }
            }
        }

        if (list.Count - 1 > 0)
        {
            await PlayerCmd.GainGold(250 * (list.Count - 1), Owner);
            NMerchantRoom? merchantRoom = NRun.Instance?.MerchantRoom;
            if (merchantRoom != null)
            {
                LocString line1 = new LocString("merchant_room", "RUINA2-MERCHANT.talk.ashes.line2");
                merchantRoom.MerchantButton.PlayDialogue(line1);
            }
        }
    }
}