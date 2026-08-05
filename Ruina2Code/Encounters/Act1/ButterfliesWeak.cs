using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class ButterfliesWeak : CustomEncounterModel
{
    public ButterfliesWeak() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["butterfly_bg.tscn".BackgroundImagePath()], 
            "butterfly_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "butterflies_weak.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override bool IsWeak => true;
    public override IEnumerable<EncounterTag> Tags => [RuinaEncounterTags.Butterflies];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Butterflies>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Butterflies>().ToMutable(), "slot1"),
            (ModelDb.Monster<Butterflies>().ToMutable(), "slot2"),
            (ModelDb.Monster<Butterflies>().ToMutable(), "slot3")
        };
    }
}