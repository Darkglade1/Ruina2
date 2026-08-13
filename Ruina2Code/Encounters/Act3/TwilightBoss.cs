using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act3.Twilight;

namespace Ruina2.Ruina2Code.Encounters.Act3;

public sealed class TwilightBoss : CustomEncounterModel
{
    public TwilightBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "Twilight/TwilightMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "Twilight/TwilightIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "Twilight/TwilightIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["twilight_bg.tscn".BackgroundImagePath()], 
            "twilight_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "twilight_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<BigEgg>();
            yield return ModelDb.Monster<SmallEgg>();
            yield return ModelDb.Monster<LongEgg>();
            yield return ModelDb.Monster<Twilight>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<BigEgg>().ToMutable(), "egg1"),
            (ModelDb.Monster<SmallEgg>().ToMutable(), "egg2"),
            (ModelDb.Monster<LongEgg>().ToMutable(), "egg3"),
            (ModelDb.Monster<Twilight>().ToMutable(), "boss")
        };
    }
}