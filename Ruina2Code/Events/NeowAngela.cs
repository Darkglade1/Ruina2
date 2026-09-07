using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using Ruina2.Ruina2Code.Audio;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Relics;

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
      List<EventOption> items = ModelDb.AncientEvent<Neow>().AllPossibleOptions.ToList();
      items.Add(GlimpseOfEgoOption);
      items.Add(BookOfEgoOption);
      return items;
    }
  }

  public IEnumerable<EventOption> PositiveOptions
  {
    get
    {
      List<EventOption> neowItems = ModelDb.AncientEvent<Neow>().PositiveOptions.ToList();
      List<EventOption> items = new List<EventOption>();
      foreach (var neowItem in neowItems)
      {
        var relic = neowItem.Relic;
        if (relic != null)
        {
          items.Add(RelicOption(relic, customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
        }
      }
      items.Add(GlimpseOfEgoOption);
      return items;
    }
  }

  public IEnumerable<EventOption> CurseOptions
  {
    get
    {
      List<EventOption> neowItems = ModelDb.AncientEvent<Neow>().CurseOptions.ToList();
      List<EventOption> items = new List<EventOption>();
      foreach (var neowItem in neowItems)
      {
        var relic = neowItem.Relic;
        if (relic != null)
        {
          items.Add(RelicOption(relic, customDonePage: "NEOW.pages.DONE.CURSED.description"));
        }
      }
      items.Add(BookOfEgoOption);
      return items;
    }
  }
  
  public EventOption GlimpseOfEgoOption
  {
    get => RelicOption<GlimpseOfEgo>(customDonePage: "NEOW.pages.DONE.POSITIVE.description");
  }
  
  public EventOption BookOfEgoOption
  {
    get => RelicOption<BookOfEgo>(customDonePage: "NEOW.pages.DONE.CURSED.description");
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
      if (eventOption?.Relic is BookOfEgo)
        list2.RemoveAll((Predicate<EventOption>) (o => o.Relic is GlimpseOfEgo));
      if (!(eventOption?.Relic is LargeCapsule))
      {
        if (Rng.NextBool())
          list2.Add(RelicOption<LavaRock>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
        else
          list2.Add(RelicOption<SmallCapsule>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
      }
      if (Rng.NextBool())
        list2.Add(RelicOption<NutritiousOyster>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
      else
        list2.Add(RelicOption<StoneHumidifier>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
      if (Rng.NextBool())
        list2.Add(RelicOption<NeowsTalisman>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
      else
        list2.Add(RelicOption<Pomander>(customDonePage: "NEOW.pages.DONE.POSITIVE.description"));
      list2.RemoveAll((Predicate<EventOption>) (r =>
      {
        RelicModel? relic = r.Relic;
        return relic != null && !relic.IsAllowedAtNeow(Owner);
      }));
      List<EventOption> items = new List<EventOption>();
      items.AddRange(list2.ToList().UnstableShuffle(Rng).Take(2));
      items.Add(eventOption!);
      return items;
    }
    return [];
  }
}