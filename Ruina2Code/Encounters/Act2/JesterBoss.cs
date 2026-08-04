using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act2.Jester;

namespace Ruina2.Ruina2Code.Encounters.Act2;

public sealed class JesterBoss : CustomEncounterModel
{
    public JesterBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "Jester/JesterMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "Jester/JesterIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "Jester/JesterIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["nihil_bg.tscn".BackgroundImagePath()], 
            "nihil_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "jester_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<QueenOfLove>();
            yield return ModelDb.Monster<ServantOfCourage>();
            yield return ModelDb.Monster<JesterOfNihil>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<QueenOfLove>().ToMutable(), "love"),
            (ModelDb.Monster<ServantOfCourage>().ToMutable(), "courage"),
            (ModelDb.Monster<JesterOfNihil>().ToMutable(), "jester")
        };
    }
}