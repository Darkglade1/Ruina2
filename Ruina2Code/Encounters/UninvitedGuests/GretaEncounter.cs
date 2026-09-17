using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Ruina2.Ruina2Code.Extensions;
using Ruina2.Ruina2Code.Monsters.UninvitedGuests.Greta;

namespace Ruina2.Ruina2Code.Encounters.UninvitedGuests;

public sealed class GretaEncounter : CustomEncounterModel
{
    public GretaEncounter() : base(RoomType.Elite)
    {
    }
    public override CustomBackgroundAssets? CustomEncounterBackground(ActModel parentAct, Rng rng)
    {
        return new CustomBackgroundAssets("res://BaseLib/scenes/dynamic_background.tscn",
            ["hod_bg.tscn".BackgroundImagePath()], 
            "hod_bg.tscn".BackgroundImagePath());
    }
    public override string? CustomScenePath => "greta_encounter.tscn".EncounterImagePath();
    public override float GetCameraScaling() => 0.9f;
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            yield return ModelDb.Monster<Hod>();
            yield return ModelDb.Monster<FreshMeat>();
            yield return ModelDb.Monster<Greta>();
        }
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<Hod>().ToMutable(), "ally"),
            (ModelDb.Monster<Greta>().ToMutable(), "boss")
        };
    }
}