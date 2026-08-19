using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.Oz;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class OzBoss : CustomEncounterModel
{
    public OzBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "Oz/OzMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "Oz/OzIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "Oz/OzIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["emerald_bg.tscn".BackgroundImagePath()], 
            "emerald_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "oz_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<ScowlingFace>();
            yield return ModelDb.Monster<Oz>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Oz>().ToMutable(), "oz"),
        };
    }
}