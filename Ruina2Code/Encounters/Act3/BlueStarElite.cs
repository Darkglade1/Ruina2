using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act3.BlueStar;

namespace Ruina2.Ruina2Code.Encounters.Act3;

public sealed class BlueStarElite : CustomEncounterModel
{
    public BlueStarElite() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["star_bg.tscn".BackgroundImagePath()], 
            "star_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "blue_star_elite.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Worshipper>();
            yield return ModelDb.Monster<BlueStar>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Worshipper>().ToMutable(), "minion1"),
            (ModelDb.Monster<Worshipper>().ToMutable(), "minion2"),
            (ModelDb.Monster<BlueStar>().ToMutable(), "star")
        };
    }
}