using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Eileen;

namespace Ruina2.Ruina2Code.Encounters.UninvitedGuests;

public sealed class EileenEncounter : CustomEncounterModel
{
    public EileenEncounter() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["yesod_bg.tscn".BackgroundImagePath()], 
            "yesod_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "eileen_encounter.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Yesod>();
            yield return ModelDb.Monster<GearWorshipper>();
            yield return ModelDb.Monster<Eileen>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Yesod>().ToMutable(), "ally"),
            (ModelDb.Monster<GearWorshipper>().ToMutable(), "minion1"),
            (ModelDb.Monster<GearWorshipper>().ToMutable(), "minion2"),
            (ModelDb.Monster<Eileen>().ToMutable(), "boss")
        };
    }
}