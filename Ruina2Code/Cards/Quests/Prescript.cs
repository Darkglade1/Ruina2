using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Ruina2.Ruina2Code.RestSite;

namespace Ruina2.Ruina2Code.Cards.Quests;

[Pool(typeof(QuestCardPool))]
public class Prescript() : Ruina2Card(-1, CardType.Quest,
    CardRarity.Quest, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public override int MaxUpgradeLevel => 0;
    
    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
            return false;
        options.Add(new PrescriptRestSiteOption(player));
        return true;
    }
}