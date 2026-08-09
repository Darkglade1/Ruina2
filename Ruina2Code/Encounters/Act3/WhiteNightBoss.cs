using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.Jester;
using Ruina2.Ruina2Code.Monsters.Act3;

namespace Ruina2.Ruina2Code.Encounters.Act3;

public sealed class WhiteNightBoss : CustomEncounterModel
{
    public WhiteNightBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "WhiteNight/WhiteNightMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "WhiteNight/WhiteNightIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "WhiteNight/WhiteNightIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["paradise_bg.tscn".BackgroundImagePath()], 
            "paradise_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "white_night_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<WhiteNight>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<WhiteNight>().ToMutable(), "boss")
        };
    }
}