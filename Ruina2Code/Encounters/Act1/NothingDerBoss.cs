using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.Act1.NothingDer;
using Ruina2.Ruina2Code.Monsters.Act2.Jester;

namespace Ruina2.Ruina2Code.Encounters.Act1;

public sealed class NothingDerBoss : CustomEncounterModel
{
    public NothingDerBoss() : base(RoomType.Boss)
    {
    }
    public override string BossNodePath => "DerFreischutz/NothingDerMap".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconPath => "DerFreischutz/NothingDerIcon.png".MonsterImagePath().SimplifyPath();
    public override string? CustomRunHistoryIconOutlinePath => "DerFreischutz/NothingDerIconOutline.png".MonsterImagePath().SimplifyPath();
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["gun_bg.tscn".BackgroundImagePath()], 
            "gun_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "nothing_der_boss.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override bool FullyCenterPlayers => true;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<NothingThere>();
            yield return ModelDb.Monster<DerFreischutz>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<NothingThere>().ToMutable(), "nothing"),
            (ModelDb.Monster<DerFreischutz>().ToMutable(), "der")
        };
    }
}