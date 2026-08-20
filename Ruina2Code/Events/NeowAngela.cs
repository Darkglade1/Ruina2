using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
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
    
    public override bool IsValidForAct(ActModel act) => false;

    protected override async Task BeforeEventStarted(bool isPreFinished)
    {
        await base.BeforeEventStarted(isPreFinished);
        if (isPreFinished)
        {
            return;
        }
        Sfx.FingerSnap.Play(0, 0.8f);
    }
    
    protected override OptionPools MakeOptionPools => new([]);
    
     public override IEnumerable<EventOption> AllPossibleOptions
  {
    get
    {
      List<EventOption> items = new List<EventOption>();
      items.AddRange(CurseOptions);
      items.AddRange(PositiveOptions);
      items.Add(LavaRockOption);
      items.Add(NeowsTalismanOption);
      items.Add(NutritiousOysterOption);
      items.Add(PomanderOption);
      items.Add(SmallCapsuleOption);
      items.Add(StoneHumidifierOption);
      return items;
    }
  }

  public IEnumerable<EventOption> PositiveOptions
  {
    get
    {
      return new[]
      {
        RelicOption<ArcaneScroll>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<BoomingConch>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<FishingRod>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<GoldenPearl>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<Kaleidoscope>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<LeadPaperweight>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<LostCoffer>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<MassiveScroll>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<NeowsTorment>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<NewLeaf>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<PhialHolster>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<PreciseScissors>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<ScrollBoxes>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<WingedBoots>(customDonePage: "NEOW.pages.DONE.POSITIVE.description")
      };
    }
  }

  public EventOption LavaRockOption
  {
    get => RelicOption<LavaRock>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
  }

  public EventOption NeowsTalismanOption
  {
    get => RelicOption<NeowsTalisman>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
  }

  public EventOption NutritiousOysterOption
  {
    get
    {
      return RelicOption<NutritiousOyster>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
    }
  }

  public EventOption PomanderOption
  {
    get => RelicOption<Pomander>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
  }

  public EventOption SmallCapsuleOption
  {
    get => RelicOption<SmallCapsule>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
  }

  public EventOption StoneHumidifierOption
  {
    get
    {
      return RelicOption<StoneHumidifier>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
    }
  }

  public IEnumerable<EventOption> CurseOptions
  {
    get
    {
      return new []
      {
        RelicOption<CursedPearl>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<DowsingRod>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<HeftyTablet>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<LargeCapsule>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<LeafyPoultice>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<NeowsBones>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"),
        RelicOption<NeowsSacrifice>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<PrecariousShears>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<SilkenTress>(customDonePage: "NEOW.pages.DONE.CURSED.description"),
        RelicOption<SilverCrucible>(customDonePage: "NEOW.pages.DONE.CURSED.description")
      };
    }
  }
    
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
  {
    if (Owner?.RunState.Modifiers.Count <= 0)
    {
      List<EventOption> list1 = CurseOptions.ToList();
      list1.RemoveAll((Predicate<EventOption>) (r =>
      {
        RelicModel? relic = r.Relic;
        return relic != null && !relic.IsAllowedAtNeow(Owner);
      }));
      EventOption? eventOption = Rng.NextItem(list1);
      List<EventOption> list2 = PositiveOptions.ToList();
      if (eventOption?.Relic is CursedPearl)
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is GoldenPearl));
      if (eventOption?.Relic is HeftyTablet)
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is ArcaneScroll));
      if (eventOption?.Relic is LeafyPoultice)
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is NewLeaf));
      if (eventOption?.Relic is PrecariousShears)
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is PreciseScissors));
      if (eventOption?.Relic is NeowsSacrifice)
      {
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is PhialHolster));
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is LostCoffer));
      }
      if (!(eventOption?.Relic is LargeCapsule))
      {
        if (Rng.NextBool())
          list2.Add(LavaRockOption);
        else
          list2.Add(SmallCapsuleOption);
      }
      if (Rng.NextBool())
        list2.Add(NutritiousOysterOption);
      else
        list2.Add(StoneHumidifierOption);
      if (Rng.NextBool())
        list2.Add(NeowsTalismanOption);
      else
        list2.Add(PomanderOption);
      list2.RemoveAll((Predicate<EventOption>) (r =>
      {
        RelicModel? relic = r.Relic;
        return relic != null && !relic.IsAllowedAtNeow(Owner);
      }));
      List<EventOption> items = new List<EventOption>();
      items.AddRange(list2.ToList().UnstableShuffle(Rng).Take(2));
      items.Add(eventOption);
      return items;
    }
    return [];
  }
}