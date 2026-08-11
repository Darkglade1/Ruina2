using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act3.BigBird;

namespace Ruina2.Ruina2Code.Encounters.Act3;

public sealed class BigBirdElite : CustomEncounterModel
{
    public BigBirdElite() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["black_forest_bg.tscn".BackgroundImagePath()], 
            "black_forest_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "big_bird_elite.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Sage>();
            yield return ModelDb.Monster<BigBird>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Sage>().ToMutable(), "sage1"),
            (ModelDb.Monster<Sage>().ToMutable(), "sage2"),
            (ModelDb.Monster<BigBird>().ToMutable(), "bird")
        };
    }
}