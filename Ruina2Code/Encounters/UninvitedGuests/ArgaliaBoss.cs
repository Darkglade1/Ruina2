using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Argalia;

namespace Ruina2.Ruina2Code.Encounters.UninvitedGuests;

public sealed class ArgaliaBoss : CustomEncounterModel
{
    public ArgaliaBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "Argalia/ArgaliaMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "Argalia/ArgaliaIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "Argalia/ArgaliaIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["keter_bg.tscn".BackgroundImagePath()], 
            "keter_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "argalia_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Roland>();
            yield return ModelDb.Monster<Argalia>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Roland>().ToMutable(), "ally"),
            (ModelDb.Monster<Argalia>().ToMutable(), "boss")
        };
    }
}