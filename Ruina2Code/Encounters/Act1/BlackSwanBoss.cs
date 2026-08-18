using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1.BlackSwan;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class BlackSwanBoss : CustomEncounterModel
{
    public BlackSwanBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "BlackSwan/BlackSwanMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "BlackSwan/BlackSwanIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "BlackSwan/BlackSwanIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["swan_bg.tscn".BackgroundImagePath()], 
            "swan_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "black_swan_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Brother>();
            yield return ModelDb.Monster<BlackSwan>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<BlackSwan>().ToMutable(), "swan")
        };
    }
}