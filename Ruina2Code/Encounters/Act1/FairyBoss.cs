using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1.FairyFestival;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class FairyBoss : CustomEncounterModel
{
    public FairyBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "FairyQueen/FairyMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "FairyQueen/FairyIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "FairyQueen/FairyIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["fairy_bg.tscn".BackgroundImagePath()], 
            "fairy_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "fairy_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<FairyMass>();
            yield return ModelDb.Monster<FairyQueen>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<FairyMass>().ToMutable(), "minion1"),
            (ModelDb.Monster<FairyMass>().ToMutable(), "minion2"),
            (ModelDb.Monster<FairyQueen>().ToMutable(), "fairy")
        };
    }
}