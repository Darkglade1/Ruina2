using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.Knight;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class KnightNormal : CustomEncounterModel
{
    public KnightNormal() : base(RoomType.Monster)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["despair_night_bg.tscn".BackgroundImagePath()], 
            "despair_night_bg.tscn".BackgroundImagePath());
    }
    
    public override IReadOnlyList<string> Slots =>
    [
        "sword",
        "knight"
    ];

    public override string? CustomScenePath => "knight_normal.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => [];
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<KnightOfDespair>();
            yield return ModelDb.Monster<Sword>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Sword>().ToMutable(), "sword"),
            (ModelDb.Monster<KnightOfDespair>().ToMutable(), "knight")
        };
    }
}