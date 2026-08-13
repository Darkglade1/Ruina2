using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.Relics;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;

namespace Ruina2.Ruina2Code.Events;
public class NeowAngela : CustomAncientModel
{
    public override string CustomScenePath => "Angela/angela.tscn".EventImagePath();
    public override string CustomMapIconPath => "Angela/AngelaMap.png".EventImagePath();
    public override string CustomMapIconOutlinePath => "Angela/AngelaMapOutline.png".EventImagePath();
    public override string CustomRunHistoryIconPath => "Angela/AngelaIcon.png".EventImagePath();
    public override string CustomRunHistoryIconOutlinePath => "Angela/AngelaIconOutline.png".EventImagePath();

    protected override async Task BeforeEventStarted(bool isPreFinished)
    {
        await base.BeforeEventStarted(isPreFinished);
        if (isPreFinished)
        {
            return;
        }
        Sfx.FingerSnap.Play(0, 0.8f);
    }
    
    // TODO: Add exclusive logic that basegame Neow has.
    protected override OptionPools MakeOptionPools => new(
        MakePool(
          AncientOption<LavaRock>(),
          AncientOption<NeowsTalisman>(),
          AncientOption<NutritiousOyster>(),
          AncientOption<Pomander>(),
          AncientOption<SmallCapsule>(),
          AncientOption<StoneHumidifier>(),
          AncientOption<ArcaneScroll>(),
          AncientOption<BoomingConch>(),
          AncientOption<FishingRod>(),
          AncientOption<GoldenPearl>(),
          AncientOption<Kaleidoscope>(),
          AncientOption<LeadPaperweight>(),
          AncientOption<LostCoffer>(),
          AncientOption<MassiveScroll>(),
          AncientOption<NeowsTorment>(),
          AncientOption<NewLeaf>(),
          AncientOption<PhialHolster>(),
          AncientOption<PreciseScissors>(),
          AncientOption<ScrollBoxes>(),
          AncientOption<WingedBoots>()
        ),
        MakePool(
          AncientOption<CursedPearl>(),
            AncientOption<DowsingRod>(),
            AncientOption<HeftyTablet>(),
            AncientOption<LargeCapsule>(),
            AncientOption<LeafyPoultice>(),
            AncientOption<NeowsBones>(),
            AncientOption<NeowsSacrifice>(),
            AncientOption<PrecariousShears>(),
            AncientOption<SilkenTress>(),
            AncientOption<SilverCrucible>()
        ));
}