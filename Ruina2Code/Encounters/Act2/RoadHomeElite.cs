using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.RoadHome;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class RoadHomeElite : CustomEncounterModel
{
    public RoadHomeElite() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["road_bg.tscn".BackgroundImagePath()], 
            "road_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "road_home_elite.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Home>();
            yield return ModelDb.Monster<ScaredyCat>();
            yield return ModelDb.Monster<RoadHome>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Home>().ToMutable(), "home"),
            (ModelDb.Monster<ScaredyCat>().ToMutable(), "cat"),
            (ModelDb.Monster<RoadHome>().ToMutable(), "road")
        };
    }
}